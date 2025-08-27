using System;
using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using TMPro;
using UnityEngine;

[Serializable]
public class ReportTableCol
{
    public List<TextMeshProUGUI> tmps; // 0:증류수, 1:아황산 나트륨, 2:이온산 칼륨, 3:반응 시간
}

public class ClockReactionReport : EPReport
{
    [SerializeField] List<ReportTableCol> table = new(); // 인스펙터에서 2줄(row) 셋업 권장

    [Header("Format")]
    [SerializeField] string amountFormat = "0";   // 양 표시 포맷
    [SerializeField] string timeFormat = "0.#"; // 시간 표시 포맷

    [Button("TestReporting"), HideField] public bool testButton1;

    public void TestReporting()
    {
        WriteResult();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Pen"))
        {
            WriteResult();
        }

        if (collision.gameObject.tag.Equals("NPC"))
        {
            // 리포트 제출 텍스트 출력
            GeminiAPIManager.Instance.SendMessage("Report_ClockReaction");
        }
    }

    public override void WriteResult()
    {
        base.WriteResult();

        var em = ExperimentManager.Instance;
        if (em == null || em.CachedData_ClockReaction == null) return;

        List<ClockReactionCase> cachedData = em.CachedData_ClockReaction;

        // 테이블 줄 수만큼 반복(보통 2줄: case1, case2)
        for (int i = 0; i < table.Count; i++)
        {
            ReportTableCol row = table[i];
            if (row == null || row.tmps == null || row.tmps.Count < 4) continue;

            if (i < cachedData.Count) // 해당 case가 존재 → 채우기
            {
                var c = cachedData[i];

                float distilled = GetAmountSafe(c, ChemFlag.Distilled);
                float sodium = GetAmountSafe(c, ChemFlag.Sulfite_Sodium);
                float potassium = GetAmountSafe(c, ChemFlag.Iodine_K);
                float t = Mathf.Max(0f, c.reactionTime);

                row.tmps[0].text = distilled.ToString(amountFormat);
                row.tmps[1].text = sodium.ToString(amountFormat);
                row.tmps[2].text = potassium.ToString(amountFormat);
                row.tmps[3].text = t.ToString(timeFormat);
            }
            else // case 미존재 → 0으로 클리어
            {
                row.tmps[0].text = 0f.ToString(amountFormat);
                row.tmps[1].text = 0f.ToString(amountFormat);
                row.tmps[2].text = 0f.ToString(amountFormat);
                row.tmps[3].text = 0f.ToString(timeFormat);
            }
        }

        Debug.Log("시계반응 리포트 갱신 완료");
    }

    // 안전한 양 조회(성분 없으면 0)
    float GetAmountSafe(ClockReactionCase c, ChemFlag flag)
    {
        if (c == null || c.chemInforms == null) return 0f;
        var info = c.chemInforms.FirstOrDefault(x => x != null && x.flag == flag);
        return info != null ? info.amount : 0f;
    }
}
