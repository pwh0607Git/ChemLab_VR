using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChemFlag
{
    None,
    Distilled,              // 증류수
    Iodine_K,               // 요오드산 칼륨 [녹말 추가할 용액]
    Sulfite_Sodium,         // 아황산수소나트륨
    Starch,                 // 녹말 
}

[CreateAssetMenu(fileName = "ChemicalSet")]
public class ChemicalData : ScriptableObject
{
    public ChemFlag flag;
    public Color baseColor;
}
