// ClockResetManager.cs
using UnityEngine;

public class ClockResetManager : MonoBehaviour
{
    [Tooltip("시계반응 비커들을 직접 드래그해서 넣어두면 여기에 있는 것만 리셋합니다.")]
    public Beaker[] beakers;

    [Tooltip("배열이 비어있으면 이 태그로 씬에서 자동 검색합니다.")]
    public string fallbackTag = "ClockBeaker";

    public void SoftResetClockOnly()
    {
        bool any = false;

        // 1) 배열에 지정된 비커가 있으면 그 비커들만 리셋
        if (beakers != null && beakers.Length > 0)
        {
            foreach (var b in beakers)
            {
                if (!b) continue;
                b.ResetForNextRun();   // Beaker에 구현한 소프트리셋 메서드
                any = true;
            }
        }

        // 2) 배열이 비어있거나 null이면 태그로 자동 검색해서 리셋
        if (!any && !string.IsNullOrEmpty(fallbackTag))
        {
            var tagged = GameObject.FindGameObjectsWithTag(fallbackTag);
            foreach (var go in tagged)
            {
                var b = go.GetComponent<Beaker>();
                if (!b) continue;
                b.ResetForNextRun();
            }
        }

        Debug.Log("[ClockResetManager] SoftResetClockOnly done.");
    }
}
