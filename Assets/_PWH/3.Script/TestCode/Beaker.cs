using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using DG.Tweening;
using UnityEngine;

public enum ChemFlag {
    None,
    Distilled,              // 증류수
    Iodine_K,               // 요오드산 칼륨 [녹말 추가할 용액]
    Sulfite_Sodium,         // 아황산수소나트륨
    Starch,                 // 녹말 
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
    [Header("Beaker Property")]
    [SerializeField] private float beakerAmount;             //비커 수용량
    [SerializeField, ReadOnly] private float currentAmount;
    [SerializeField] private Renderer liquidRender;

    [Header("Blend State")]
    [SerializeField] private List<ChemInform> blendedLiquid = new();          //비커에 들어있는 혼합용액 상태.
    [SerializeField] private List<ChemInform> blendedPowder = new();          //비커에 들어있는 혼합가루 상태.

    public List<ChemInform> BlendedLiquid => blendedLiquid;
    public List<ChemInform> BlendedPowder => blendedPowder;

    void Start()
    {
        Reset();
        liquidRender.material.SetFloat("_Fill", currentAmount / beakerAmount);
    }

    void Update()
    {
        if (isPour)
        {
            PourBlend();
        }
    }

    // 추가할 화학용품, 증가량
    public void AddLiquid(ChemFlag flag, float add)
    {
        if (flag.Equals(ChemFlag.None)) return;
        if (currentAmount >= beakerAmount) return;

        ChemInform chem = blendedLiquid.Find(b => b.flag == flag);

        //혼합물에 이미 있는지 확인
        if (chem == null)
        {
            chem = new ChemInform(flag);
            blendedLiquid.Add(chem);
        }

        chem.amount += add;
        currentAmount += add;

        //fill amount 조절하기
        liquidRender.material.SetFloat("_Fill", currentAmount / beakerAmount);
    }

    public void AddPowder(ChemFlag flag, int add)
    {
        if (flag.Equals(ChemFlag.None)) return;

        ChemInform chem = blendedPowder.Find(b => b.flag == flag);

        //혼합물에 이미 있는지 확인
        if (chem == null)
        {
            chem = new ChemInform(flag);
            blendedLiquid.Add(chem);
        }
        chem.amount++;
    }

    public void Reset()
    {
        currentAmount = 0f;

        blendedLiquid.Clear();
        blendedPowder.Clear();
    }

    [Header("Material Test")]
    public Color colorTest;

    [Button("ChangeColor"), HideField] public bool b1;
    public void ChangeColor()
    {
        liquidRender.material.SetColor("_SideColor", colorTest);
        liquidRender.material.SetColor("_TopColor", colorTest);
    }

    [Header("Pour test")]
    [SerializeField] float pourPerFrame;

    bool isPour = false;

    [SerializeField] float noneMixTime;             // 혼합물과 섞이지 않은 시간.

    public void PourBlend()
    {
        if (currentAmount <= 0) return;
        currentAmount -= pourPerFrame;

        foreach (var b in blendedLiquid)
        {
            if (b.amount <= 0)
            {
                blendedLiquid.Remove(b);
                continue;
            }
            b.amount -= pourPerFrame / blendedLiquid.Count;
        }

        liquidRender.material.SetFloat("_Fill", currentAmount / beakerAmount);
    }

    void MixBlend(List<ChemInform> mixture)
    {
        Debug.Log(mixture.Count());
        foreach (var m in mixture)
        {
            AddLiquid(m.flag, m.amount);
        }
    }

    void StartChemicalReaction()
    {
        // 칼륨과 나트륨이 모두 있을 경우에만 화학 반응이 작용한다.
        if (blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Iodine_K)) == null || blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Sulfite_Sodium)) == null) return;

        Debug.Log("화학 반응 수행!");
        float t = 1f;

        DOVirtual.DelayedCall(t, () =>
        {
            ChangeColor();
        });
    }

    /*
        Blend가 혼합 되었을 때 완료 되면 호출한다.
        Observer Pattern.
    */


    [SerializeField] private List<ChemInform> testLiquid = new();          //비커에 들어있는 혼합용액 상태.
    [SerializeField] private List<ChemInform> testPowder = new();          //비커에 들어있는 혼합가루 상태.

    void MixTest()
    {
        foreach (var m in testLiquid)
        {
            AddLiquid(m.flag, m.amount);
        }


        foreach (var m in testPowder)
        {
            AddPowder(m.flag, (int)m.amount);
        }
    }

    [Button("MixTest"), HideField] public bool btn2;



    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Beaker"))
        {
            Debug.Log("Beaker와 부딪힘.");

            Beaker b = collision.gameObject.GetComponent<Beaker>();
            var blendedLiquid = b.BlendedLiquid;
            MixBlend(blendedLiquid);

            //test
            StartChemicalReaction();
        }
    }
    
    
    // 파티클이 비커의 콜라이더와 충돌할 때 호출
    void OnParticleCollision(GameObject other)
    {
        // 충돌한 파티클 시스템이 '물 파티클'인지 확인하는 로직 추가 가능
        if (other.name.Equals("LiquidPour"))
        {
            //혼합물인지 일반 liquid 인지 체크
            var liquid = other.GetComponentInParent<Liquid>();

            if (liquid != null)
            {
                AddLiquid(liquid.Flag, 0.2f);
            }
        }
    }
}