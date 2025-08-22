using System;
using System.Collections.Generic;
using CustomInspector;
using UnityEngine;

// 화학 시계 반응 실험
[Serializable]
public class ClockReactionCase
{
    public List<ChemInform> chemInforms = new();
    public float reactionTime;

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


    [Header("Clock Reaction")]
    [SerializeField, ReadOnly] List<ClockReactionCase> cachedData_ClockReaction;
    public List<ClockReactionCase> CachedData_ClockReaction => cachedData_ClockReaction;

    [Header("Explosion Reaction")]
    public string res;

    public void UpdateExperiment(List<ChemInform> informs, float t1)
    {
        ClockReactionCase case1 = new();

        foreach (var i in informs)
        {
            case1.AddData(i.flag, i.amount);
        }

        case1.reactionTime = t1;
        cachedData_ClockReaction.Add(case1);
    }
}