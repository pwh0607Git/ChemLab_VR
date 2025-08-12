using System.Collections;
using System.Collections.Generic;
using CustomInspector;
using UnityEngine;

public enum ChemFlag {
    Distilled,              // 증류수
    Iodine_K,               // 요오드산 칼륨
    Sulfite_Sodium,         // 아황산수소나트륨
    Starch,                 //녹말 
}

[System.Serializable]
public class ChemInform
{
    public ChemFlag flag;
    public float amount;

    public ChemInform(ChemFlag flag)
    {
        this.flag = flag;
        amount = 0f;
    }
}

public class Beaker : MonoBehaviour
{
    //비커에 들어있는 혼합물 상태.
    [SerializeField, ReadOnly] private List<ChemInform> blend = new();

    public List<ChemInform> Blend => blend;

    // 추가할 화학용품, 증가량
    public void AddChem(ChemFlag flag, float add)
    {
        ChemInform chem = blend.Find(b => b.flag == flag);

        //혼합물에 이미 있는지 확인
        if (chem == null)
        {
            blend.Add(new ChemInform(flag));
        }

        chem.amount += add;
    }

    [SerializeField, ReadOnly] GameObject liquid;
    [SerializeField] Renderer ren;

    // 파티클이 비커의 콜라이더와 충돌할 때 호출
    void OnParticleCollision(GameObject other)
    {
        // 충돌한 파티클 시스템이 '물 파티클'인지 확인하는 로직 추가 가능
        if (other.name.Equals("LiquidPour"))
        {
            float amount = ren.material.GetFloat("_Fill");

            amount += 0.001f;

            //fill amount 조절하기
            ren.material.SetFloat("_Fill", amount);
        }
    }
}