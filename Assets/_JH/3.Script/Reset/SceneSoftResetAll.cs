using UnityEngine;

public class SceneSoftResetAll : MonoBehaviour
{
    [Tooltip("리셋 전에 비활성 포함 전부 스캔")]
    public bool includeInactive = true;

    public void SoftResetAll()
    {
        var targets = FindObjectsOfType<MonoBehaviour>(includeInactive);
        int count = 0;
        foreach (var mb in targets)
        {
            if (mb is ISoftResettable r)
            {
                r.SoftReset();
                count++;
            }
        }
        Debug.Log($"[SceneSoftResetAll] SoftReset 호출 대상: {count}개");
    }
}
