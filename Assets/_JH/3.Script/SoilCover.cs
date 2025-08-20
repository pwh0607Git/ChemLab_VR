using UnityEngine;

public class SoilCover : MonoBehaviour
{
    public enum ConditionSource { None, EmitterDuration, TriggerAccumulation }

    [Header("덮을 오브젝트")]
    public GameObject soilMesh;

    [Header("조건 소스 선택")]
    public ConditionSource condition = ConditionSource.TriggerAccumulation;

    [Header("EmitterDuration 사용 시")]
    public FeedPowderEmitter powderEmitter;
    public float requiredPourTime = 2f;

    [Header("TriggerAccumulation 사용 시")]
    public PowderTriggerScaler triggerScaler;
    [Range(0, 1)] public float requiredFill01 = 0.3f;

    private bool isCovered = false;
    public UnityEngine.Events.UnityEvent onCovered;

    // XR 버튼/OnClick에서 호출
    public void CoverSoil()
    {
        if (isCovered) { Debug.Log("[SoilCover] 이미 덮음"); return; }
        if (!soilMesh) { Debug.LogError("[SoilCover] soilMesh 미지정"); return; }

        // 조건 체크
        switch (condition)
        {
            case ConditionSource.EmitterDuration:
                if (!powderEmitter) { Debug.LogError("[SoilCover] powderEmitter 미지정"); return; }
                if (powderEmitter.pourDuration < requiredPourTime)
                {
                    Debug.LogWarning($"[SoilCover] 아직 부족: {powderEmitter.pourDuration:0.00}s / 필요 {requiredPourTime:0.00}s");
                    return;
                }
                break;

            case ConditionSource.TriggerAccumulation:
                if (!triggerScaler) { Debug.LogError("[SoilCover] triggerScaler 미지정"); return; }
                if (triggerScaler.Fill01 < requiredFill01)
                {
                    Debug.LogWarning($"[SoilCover] 아직 부족: fill={triggerScaler.Fill01:0.00} / 필요 {requiredFill01:0.00}");
                    return;
                }
                break;

            case ConditionSource.None:
                // 무조건 허용
                break;
        }

        soilMesh.SetActive(true);
        isCovered = true;
        onCovered?.Invoke(); // 여기서 신호 발사
        Debug.Log("[SoilCover] 흙으로 덮었습니다!");
    }

    // (선택) TriggerScaler.onCoverReady 에 연결해서 버튼을 자동 활성화할 때 사용
    public void EnableCoverNow()
    {
        // UI 버튼 활성/텍스트 변경 등을 여기서 처리 가능
        Debug.Log("[SoilCover] 커버 조건 충족!");
    }
}
