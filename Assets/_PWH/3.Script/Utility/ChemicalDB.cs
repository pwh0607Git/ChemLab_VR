using System.Collections.Generic;
using UnityEngine;

public class ChemicalDB : BehaviourSingleton<ChemicalDB>
{
    protected override bool IsDontDestroy() => false;

    [SerializeField] List<ChemicalData> datas = new();

    public ChemicalData GetData(ChemFlag flag)
    {
        ChemicalData data = datas.Find(d => d.flag.Equals(flag));

        return data;
    }
}