using System;
using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using TMPro;
using UnityEngine;

[Serializable]
public class ReportTableCol
{
    public List<TextMeshProUGUI> tmps;
}

public class ClockReactionReport : EPReport
{
    [SerializeField] List<ReportTableCol> table = new();

    [Button("TestReporting"), HideField] public bool testButton1;

    public void TestReporting()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Pen"))
        {
            WriteResult();
        }   
    }

    void WriteResult()
    {
        Debug.Log("리포트 작성!");
        List<ClockReactionCase> cachedData = ExperimentManager.Instance.CachedData_ClockReaction;

        if (cachedData == null) return;

        for (int i = 0; i < cachedData.Count(); i++)
        {
            ReportTableCol col = table[i];
            col.tmps[0].text = cachedData[i].chemInforms.Find(c => c.flag.Equals(ChemFlag.Distilled)).amount.ToString();
            col.tmps[1].text = cachedData[i].chemInforms.Find(c => c.flag.Equals(ChemFlag.Sulfite_Sodium)).amount.ToString();
            col.tmps[2].text = cachedData[i].chemInforms.Find(c => c.flag.Equals(ChemFlag.Iodine_K)).amount.ToString();
            col.tmps[3].text = cachedData[i].reactionTime.ToString();
        }     
    }
}