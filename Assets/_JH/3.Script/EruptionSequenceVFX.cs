using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EruptionSequenceVFX : MonoBehaviour
{
    [Header("트리거 소스")]
    public WickIgnitable ignitable;

    [Header("심지 타는 시간 참조")]
    public BurnDownWick burnRef;

    [Header("기준/더미")]
    public Transform centerPoint; // 분출 위치 기준
    public Transform ashHeap;
    public Transform ashMesh;

    [Header("VFX Flags( VFXManager에서 등록한 것")]
    public VFXFlag flameBurstFlag = VFXFlag.Flame; // 짧은화염
    public VFXFlag smokeLoopFlag = VFXFlag.Smoke; // 루프 연기
    public VFXFlag ashFallFlag = VFXFlag.Ash; // 떨어지는 재

    [Header("루프 설정")]
    public bool flameLoop = false;  // 버스트는 보통 false
    public bool smokeLoop = true;
    public bool ashLoop = true;

    [Header("타이밍")]
    public float warmup = 0.6f; // 점화 후 예열
    public float eruptionDuration = 4.0f; // 분출 시간
    public float smolderDuration = 3.0f; // 잔연기 시간

    [Header("재 더미 성장")]
    public Vector2 targetScaleXZ = new Vector2(0.4f, 0.4f);
    public float targetHeight = 0.15f;
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float smooth = 8f;

    [Header("옵션")]
    public bool keepAshMeshAtEnd = true;  // 끝나도 메쉬 유지

    // 내부 상태
    Vector3 _heapInitScale, _meshInitScale;
    bool _running;

    private void Awake()
    {
        if (!ignitable) ignitable = FindObjectOfType<WickIgnitable>();
        if (!ashMesh) ashMesh = ashHeap;
    }

    private void OnEnable()
    {
        if (ignitable) ignitable.onIgnited.AddListener(StartEruption);
    }
    private void OnDisable()
    {
        if (ignitable) ignitable.onIgnited.RemoveListener(StartEruption);
    }
    public void AttachTocenter(VFX v)
    {
        if (!v) return;
        var t = v.transform;
        t.SetParent(centerPoint, false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public void StartEruption()
    {
        if (_running) return;
        if(!ashHeap) { Debug.LogWarning("[Eruption]  ashHeap 미지정"); return; }
        if(!centerPoint) { Debug.LogWarning("[Eruption]  centerPoint 미지정"); return; }

        _heapInitScale = ashHeap.localScale;
        _meshInitScale = ashMesh ? ashMesh.localScale : Vector3.one;

        ashMesh.localScale = Vector3.zero;
        // 시작 시에는 보이지 않게
        if (ashMesh) 
        { 
            ashMesh.localScale = Vector3.zero;
            ashMesh.gameObject.SetActive(false);
        }

        StartCoroutine(CoErupt());
    }

    IEnumerator CoErupt()
    {
        _running = true;

        // 0) 예열
        float delay = burnRef ? burnRef.burnDuration : warmup;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // 더미 보이게 전환
        if (ashMesh) ashMesh.gameObject.SetActive(true);

        // 1) VFX 스폰
        VFX flame = null, smoke = null, ash = null;
        if(VFXManager.Instance != null)
        {
            var pos = centerPoint.position;
            var rot = centerPoint.rotation;
            var par = centerPoint;

            flame = VFXManager.Instance.SpawnVFX(flameBurstFlag, pos, rot, par, flameLoop);
            AttachTocenter(flame);
            smoke = VFXManager.Instance.SpawnVFX(smokeLoopFlag, pos, rot, par, smokeLoop);
            AttachTocenter(smoke);
            ash = VFXManager.Instance.SpawnVFX(ashFallFlag, pos, rot, par, ashLoop);
            AttachTocenter(ash);
        }
        else
        {
            Debug.LogWarning("[Eruption] VFXManager.Instance 없음");
        }

        // 2) 분출 : 더미 성장
        var target = new Vector3(targetScaleXZ.x, targetHeight, targetScaleXZ.y);

        float t = 0f;
        while ( t< eruptionDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / eruptionDuration);
            float g = growthCurve.Evaluate(k);

            var desired = Vector3.Lerp(_heapInitScale, target, g);
            ashHeap.localScale = Vector3.Lerp(ashHeap.localScale, desired, Time.deltaTime * smooth);
           
            if(ashMesh)
            {
                var desiredMesh = Vector3.Lerp(_meshInitScale, target, g);
                ashMesh.localScale = Vector3.Lerp(ashMesh.localScale, desiredMesh, Time.deltaTime * smooth);
            }
            yield return null;
        }

        // 3) 잔연기 유지
        if (smolderDuration > 0f)
            yield return new WaitForSeconds(smolderDuration);

        // 4) 정리 (Stop -> 풀반납)
        if (flame) flame.Stop();
        if (ash) ash.Stop();
        if (smoke) smoke.Stop();

        if(!keepAshMeshAtEnd)
        {
            if (ashMesh) ashMesh.gameObject.SetActive(false);
            ashHeap.localScale = _heapInitScale;
        }

        _running = false;
    }
}
