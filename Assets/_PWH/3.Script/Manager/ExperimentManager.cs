using System;
using System.Collections.Generic;
using UnityEngine;

// 화학 시계 반응 실험
[Serializable]
public class ClockReactionCase
{
    public List<ChemInform> chemInforms;

    public void AddData(ChemFlag flag, float amount)
    {
        ChemInform inform = new(flag, amount);
        chemInforms.Add(inform);
    }
}

//중크롬산 암모늄 실험
[Serializable]
public class ExplosionReactionCase
{
    
}

public class ExperimentManager : BehaviourSingleton<ExperimentManager>
{
    protected override bool IsDontDestroy() => false;
    [SerializeField] GameObject report;
    [SerializeField] List<ClockReactionCase> cachedData;

    public void UpdateExperiment(List<ChemInform> informs)
    {
        ClockReactionCase case1 = new();

        foreach (var i in informs)
        {
            case1.AddData(i.flag, i.amount);
        }
        
        cachedData.Add(case1);
    }
}