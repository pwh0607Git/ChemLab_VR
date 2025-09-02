using CustomInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 분출 시퀀스: 불꽃/연기/재 VFX를 스폰하고, 재 더미를 성장시킨 뒤 정리.
/// - 스폰 높이(Y 오프셋), 개별 앵커 지원
/// - 파티클 루프 강제(풀/스폰 코드가 loop=false로 덮어써도 복구)
/// - 위(+Y) 방향으로 정렬 및 미세 리프트
/// - 불꽃 크기 스케일(Transform 또는 StartSize 곱하기)
/// - VisualEffect 프리팹을 직접 스폰(파티클과 같은 타이밍)
/// </summary>
public class EruptionSequenceVFX : MonoBehaviour
{
    // ───────── 트리거/레퍼런스 ─────────
    [Header("트리거 / 참조")]
    public WickIgnitable ignitable;
    public BurnDownWick burnRef;

    [Tooltip("효과의 기본 기준점(필수). 없으면 이 Transform 사용")]
    public Transform centerPoint;

    [Header("재 더미 오브젝트")]
    public Transform ashHeap;   // 크기 변하는 빈 오브젝트(스케일 변경)
    public Transform ashMesh;   // 실제 렌더되는 메쉬(없으면 ashHeap 사용)

    [Header("재 더미 머테리얼")]
    [Tooltip("노이즈 O")] public Material growMaterial;
    [Tooltip("노이즈 X")] public Material defaultMaterial;
    [ReadOnly] public bool noiseOn = true;

    [Header("불꽃 사운드")]
    public AudioClip fireClip;
    [SerializeField, Min(0f)] float loopFadeOut = 0.25f;
    private int _loopSfxId = 0; // 루프 사운드 인스턴스 ID
    // ───────── VFX 플래그 ─────────
    [Header("VFX Flags (VFXManager에 등록된 키)")]
    public VFXFlag flameBurstFlag = VFXFlag.FlameFx2;
    public VFXFlag smokeLoopFlag = VFXFlag.Smoke;
    public VFXFlag ashFallFlag = VFXFlag.Ash;

    // ───────── 루프/타이밍 ─────────
    [Header("루프 설정")]
    public bool flameLoop = true;
    public bool smokeLoop = true;
    public bool ashLoop = true;

    [Header("타이밍")]
    [Tooltip("점화 후 예열(또는 burnRef가 있으면 그 시간 사용)")]
    public float warmup = 0.6f;
    public float eruptionDuration = 4.0f;  // 분출 유지시간
    public float smolderDuration = 3.0f;  // 잔연기 유지시간

    // ───────── 재 더미 성장 ─────────
    [Header("재 더미 성장(목표 스케일)")]
    public Vector2 targetScaleXZ = new Vector2(0.4f, 0.4f);
    public float targetHeight = 0.15f;
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float smooth = 8f;
    [Tooltip("끝나도 메쉬를 남길지")]
    public bool keepAshMeshAtEnd = true;

    // ───────── 추가: 위치/앵커/안전 옵션 ─────────
    [Header("Spawn Height Offsets (meters)")]
    [Tooltip("불꽃 생성 높이(+ 위 / - 아래)")]
    public float flameYOffset = 0.06f;
    [Tooltip("연기 생성 높이(+ 위 / - 아래)")]
    public float smokeYOffset = 0.00f;
    [Tooltip("재 생성 높이(+ 위 / - 아래)")]
    public float ashYOffset = 0.10f;

    [Header("Anchors (있으면 사용, 없으면 centerPoint)")]
    public Transform flameAnchor;
    public Transform smokeAnchor;
    public Transform ashAnchor;

    [Header("안전/연출 옵션")]
    [Tooltip("스폰/활성화 시 모든 자식 ParticleSystem.loop 를 true로 강제")]
    public bool forceLoopAll = true;
    [Tooltip("스폰 시 분출 방향을 항상 위(+Y)로 정렬(Shape의 로컬 Z를 World Up 방향으로)")]
    public bool forceOrientUp = true;
    [Tooltip("정렬 후 살짝 위로 들어올림(가림 방지, 너무 크면 과함)")]
    public float orientLift = 0.02f;

    // ───────── 추가: 불꽃 크기 스케일 ─────────
    [Header("Flame Size")]
    [Tooltip("불꽃 전체 크기 배율(1=기본)")]
    public float flameSizeScale = 2.0f;
    [Tooltip("불꽃 프리팹의 ParticleSystem/Main/Scaling Mode가 Hierarchy일 때 권장")]
    public bool useTransformScaleForFlame = true;

    // Visual Effect 직접 스폰
    [Header("Visual Effect( 직접 스폰)")]
    [Tooltip("파티클과 같은 타이밍에 켜질 VisualEffect 프리팹)")]
    public GameObject visualEffectPrefab;
    [Tooltip("비우면 flameAnchor(또는 centerPoint)를 사용")]
    public Transform visualAnchor;
    [Tooltip("VisualEffect만의 추가 y보정")]
    public float visualYOffset = 0f;

    // 내부 상태
    Vector3 _heapInitScale;
    Vector3 _meshInitScale;
    bool _running;
    GameObject _visualInst;

    // ───────── Visual Y Raise 옵션 ─────────
    [Header("Visual Raise During Growth")]
    [Tooltip("분출 성장(0→1) 동안 VisualEffect의 Y를 점진적으로 올릴지")]
    public bool raiseVisualYDuringGrowth = true;

    [Tooltip("Growth=1일 때 추가로 올릴 높이(m)")]
    public float visualRaiseHeight = 0.15f;

    [Tooltip("Growth(0~1) → Raise(0~1) 매핑 커브")]
    public AnimationCurve visualRaiseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("앵커 Transform 자체를 올릴지(기본: false면 인스턴스만 올림)")]
    public bool moveAnchorTransform = false;

    // 내부 상태(복구/기준치)
    float _visualBaseLocalY;
    Vector3 _anchorBaseLocalPos;

    void Awake()
    {
        if (!centerPoint) centerPoint = transform;
        if (!ashMesh) ashMesh = ashHeap;
        if (!ignitable) ignitable = FindObjectOfType<WickIgnitable>();
    }

    void OnEnable()
    {
        if (ignitable) ignitable.onIgnited.AddListener(StartEruption);
    }
    void OnDisable()
    {
        if (ignitable) ignitable.onIgnited.RemoveListener(StartEruption);
        CleanupVisual();
    }

    //public void EnableNoise() => ashMaterial.SetFloat("_NoiseEnabled", 1f);
    //public void DisableNoise() => ashMaterial.SetFloat("_NoiseEnabled", 0f);

    void ApplyMaterial(Material mat)
    {
        if (ashMesh != null)
        {
            var rend = ashMesh.GetComponent<Renderer>();
            if (rend != null && mat != null)
                rend.material = mat;
        }

    }

    // ───────────────── 헬퍼 ─────────────────

    // 스폰된 VFX를 앵커 하위에 배치 + Y오프셋 + 위로 정렬 + 미세 리프트
    void AttachAndPlace(VFX v, Transform anchor, float yOffset, bool orientUp, float extraLift)
    {
        if (!v || !anchor) return;
        var t = v.transform;
        t.SetParent(anchor, false);
        t.localPosition = Vector3.up * yOffset;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        if (orientUp)
        {
            // Shuriken Shape는 로컬 Z로 분출 → 로컬 Z를 World Up으로
            t.forward = Vector3.up;
            if (extraLift != 0f) t.position += Vector3.up * extraLift;
        }
    }

    // 모든 자식 파티클 루프 강제(풀/스폰 쪽에서 loop=false로 덮어써도 복구)
    static void ForceLoopingAll(Transform root, bool loop)
    {
        if (!root) return;
        var list = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in list)
        {
            var m = ps.main;
            if (m.loop != loop) m.loop = loop;
        }
    }

    // StartSize에 배수를 곱해서 크기를 변경(ScalingMode: Shape/Local만 쓰는 경우)
    static void MultiplyStartSize(Transform root, float mul)
    {
        if (!root || Mathf.Approximately(mul, 1f)) return;

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            var sz = m.startSize;

            switch (sz.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    sz.constant *= mul;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    sz.constantMin *= mul;
                    sz.constantMax *= mul;
                    break;
                case ParticleSystemCurveMode.Curve:
                    sz.curveMultiplier *= mul;
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    sz.curveMultiplier *= mul;
                    break;
            }
            m.startSize = sz;
        }
    }

    void CleanupVisual()
    {
        if (!_visualInst) return;
        var ve = _visualInst.GetComponentInChildren<VisualEffect>(true);
        if (ve) ve.Stop();
        Destroy(_visualInst);
        _visualInst = null;
    }

    // ───────────────── 실행 ─────────────────

    [ContextMenu("TEST: Start Eruption")]
    public void StartEruption()
    {
        if (_running) return;

        if (!ashHeap) { Debug.LogWarning("[Eruption] ashHeap 미지정"); return; }
        if (!centerPoint) { Debug.LogWarning("[Eruption] centerPoint 미지정"); return; }

        _heapInitScale = ashHeap.localScale;
        _meshInitScale = ashMesh ? ashMesh.localScale : Vector3.one;

        if (ashMesh)
        {
            ashMesh.localScale = Vector3.zero;      // 시작 시 숨김
            ashMesh.gameObject.SetActive(false);
            ApplyMaterial(defaultMaterial);
            //DisableNoise();
        }

        StartCoroutine(CoErupt());
    }

    IEnumerator CoErupt()
    {
        _running = true;

        // 0) 예열 (심지 연소시간 우선)
        float delay = burnRef ? burnRef.burnDuration : warmup;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (ashMesh)
        {
            ashMesh.gameObject.SetActive(true);
            ApplyMaterial(growMaterial);
            //EnableNoise();
        }

        // 1) VFX 스폰
        VFX flame = null, smoke = null, ash = null;

        var fA = flameAnchor ? flameAnchor : centerPoint;
        var sA = smokeAnchor ? smokeAnchor : centerPoint;
        var aA = ashAnchor ? ashAnchor : centerPoint;

        if (VFXManager.Instance != null)
        {
            SoundManager.Instance.PlaySFXOn(fireClip, visualAnchor, loop: true, volume: 1f, pitch: 1f);
            //  Flame
            flame = VFXManager.Instance.SpawnVFX(
                flameBurstFlag,
                fA.position + Vector3.up * flameYOffset,
                fA.rotation, fA, flameLoop);

            AttachAndPlace(flame, fA, flameYOffset, forceOrientUp, orientLift);
            if (forceLoopAll && flame) ForceLoopingAll(flame.transform, true);

            // 크기 스케일(둘 중 하나 선택)
            if (flame && flameSizeScale > 0f && !Mathf.Approximately(flameSizeScale, 1f))
            {
                if (useTransformScaleForFlame)
                {
                    // ※ 프리팹의 PS/Main/Scaling Mode = Hierarchy 권장
                    flame.transform.localScale = Vector3.one * flameSizeScale;
                }
                else
                {
                    MultiplyStartSize(flame.transform, flameSizeScale);
                }
            }

            //  Smoke
            smoke = VFXManager.Instance.SpawnVFX(
                smokeLoopFlag,
                sA.position + Vector3.up * smokeYOffset,
                sA.rotation, sA, smokeLoop);

            AttachAndPlace(smoke, sA, smokeYOffset, false, 0f);
            if (forceLoopAll && smoke) ForceLoopingAll(smoke.transform, true);

            //  Ash
            ash = VFXManager.Instance.SpawnVFX(
                ashFallFlag,
                aA.position + Vector3.up * ashYOffset,
                aA.rotation, aA, ashLoop);

            AttachAndPlace(ash, aA, ashYOffset, false, 0f);
            if (forceLoopAll && ash) ForceLoopingAll(ash.transform, true);
        }
        else
        {
            Debug.LogWarning("[Eruption] VFXManager.Instance 없음");
        }

        // 같은 타이밍에 VisualEffect 프리팹도 직접 스폰
        if (visualEffectPrefab)
        {
            var vA = visualAnchor ? visualAnchor : fA; // flame과 동일 앵커
            var pos = vA.position + Vector3.up * visualYOffset;
            var rot = vA.rotation;

            _visualInst = Instantiate(visualEffectPrefab, pos, rot, vA);

            // 로컬 기준 정리(부모 기준으로 정확히 y=visualYOffset에 위치시키기)
            var t_ = _visualInst.transform;
            t_.SetParent(vA, false);
            t_.localPosition = Vector3.up * visualYOffset; // 기준 로컬 Y를 확정
            _visualBaseLocalY = t_.localPosition.y;

            // 앵커 자체 이동을 선택한 경우, 앵커의 기준 로컬 좌표 저장
            if (moveAnchorTransform && visualAnchor)
                _anchorBaseLocalPos = visualAnchor.localPosition;

            var ve = _visualInst.GetComponentInChildren<VisualEffect>(true);
            if (ve) ve.Play();
        }

        // 2) 분출 동안 재 더미 성장
        var target = new Vector3(targetScaleXZ.x, targetHeight, targetScaleXZ.y);
        float t = 0f;
        while (t < eruptionDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / eruptionDuration);
            float g = growthCurve.Evaluate(k);

            var desiredHeap = Vector3.Lerp(_heapInitScale, target, g);
            ashHeap.localScale = Vector3.Lerp(ashHeap.localScale, desiredHeap, Time.deltaTime * smooth);

            // VisualEffect Y 올리기
            if (raiseVisualYDuringGrowth)
            {
                float yRaise01 = visualRaiseCurve.Evaluate(g);            // 0~1
                float yRaise = yRaise01 * visualRaiseHeight;            // 미터 단위

                if (moveAnchorTransform && visualAnchor)                  // 앵커 자체를 올리는 모드
                {
                    var ap = _anchorBaseLocalPos;
                    visualAnchor.localPosition = new Vector3(ap.x, ap.y + yRaise, ap.z);
                }
                else if (_visualInst)                                     // 인스턴스만 올리는(기본) 모드
                {
                    var vt = _visualInst.transform;
                    var lp = vt.localPosition;
                    // 기준 로컬 Y(_visualBaseLocalY) 위에 추가 상승량만 더함
                    vt.localPosition = new Vector3(lp.x, _visualBaseLocalY + yRaise, lp.z);
                }
            }

            if (ashMesh)
            {
                var desiredMesh = Vector3.Lerp(_meshInitScale, target, g);
                ashMesh.localScale = Vector3.Lerp(ashMesh.localScale, desiredMesh, Time.deltaTime * smooth);
            }
            yield return null;
        }

        // 3) 잔연기 유지
        if (smolderDuration > 0f) yield return new WaitForSeconds(smolderDuration);

        // 4) 정리(자연 소거)
        if (flame) flame.Stop();
        if (smoke) smoke.Stop();
        if (ash) ash.Stop();
        if (moveAnchorTransform && visualAnchor) visualAnchor.localPosition = _anchorBaseLocalPos;
        SoundManager.Instance.StopSFX(fireClip, immediate: true, loopFadeOut);

        ApplyMaterial(defaultMaterial);
        //DisableNoise();

        // VisualEffect도 함께 정리
        CleanupVisual();

        if (!keepAshMeshAtEnd)
        {
            if (ashMesh) ashMesh.gameObject.SetActive(false);
            ashHeap.localScale = _heapInitScale;
        }

        _running = false;
    }
}
