using System.Collections;
using UnityEngine;

/// <summary>
/// 분출 시퀀스: 불꽃/연기/재 VFX를 스폰하고, 재 더미를 성장시킨 뒤 정리.
/// - 스폰 높이(Y 오프셋), 개별 앵커 지원
/// - 파티클 루프 강제(풀/스폰 코드가 loop=false로 덮어써도 복구)
/// - 위(+Y) 방향으로 정렬 및 미세 리프트
/// - 불꽃 크기 스케일(Transform 또는 StartSize 곱하기)
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

    // 내부 상태
    Vector3 _heapInitScale;
    Vector3 _meshInitScale;
    bool _running;

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
        }

        StartCoroutine(CoErupt());
    }

    IEnumerator CoErupt()
    {
        _running = true;

        // 0) 예열 (심지 연소시간 우선)
        float delay = burnRef ? burnRef.burnDuration : warmup;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (ashMesh) ashMesh.gameObject.SetActive(true);

        // 1) VFX 스폰
        VFX flame = null, smoke = null, ash = null;

        var fA = flameAnchor ? flameAnchor : centerPoint;
        var sA = smokeAnchor ? smokeAnchor : centerPoint;
        var aA = ashAnchor ? ashAnchor : centerPoint;

        if (VFXManager.Instance != null)
        {
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

        if (!keepAshMeshAtEnd)
        {
            if (ashMesh) ashMesh.gameObject.SetActive(false);
            ashHeap.localScale = _heapInitScale;
        }

        _running = false;
    }
}
