using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

    public ChemInform(ChemFlag flag, float amount)
    {
        this.flag = flag;
        this.amount = amount;
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

    [Header("Concentration")]
    [SerializeField] float concentration;

    [Header("Pour Angle")]
    [SerializeField] Transform head;
    public float angleThreshold = 120f;

    public List<ChemInform> BlendedLiquid => blendedLiquid;
    public List<ChemInform> BlendedPowder => blendedPowder;

    private PourBehaviour pour;
    [SerializeField, ReadOnly] float elapsedPour = 0f;
    [SerializeField, ReadOnly] bool isReact = false;

    void Start()
    {
        Reset();
        liquidRender.material.SetFloat("_Fill", currentAmount / beakerAmount);

        pour = new PourBehaviour();
        pour.Initialize(head, angleThreshold);
    }

    void Update()
    {
        //이미 화학 반응이 일어난 것에 대해서는 무시.
        if (isReact) return;

        if (isPour)
        {
            PourBlend();
        }

        PourLiquid();

        elapsedPour += Time.deltaTime;

        if (elapsedPour > 1f)
        {
            //1초 동안 액체를 받지 않으면 화학 반응 발생
            if (!isReact)
            {
                StartChemicalReaction();
            }
        }
    }

    public void PourLiquid()
    {
        if (currentAmount <= 0.01f) return;
        pour.UpdatePour();
    }

    // 추가할 화학용품, 증가량
    public void AddLiquid(ChemFlag flag, float add)
    {
        if (flag.Equals(ChemFlag.None)) return;

        //조건 추가하기.
        // if (currentAmount >= beakerAmount) return;

        ChemInform chem = blendedLiquid.Find(b => b.flag == flag);

        //혼합물에 이미 있는지 확인
        if (chem == null)
        {
            chem = new ChemInform(flag);
            blendedLiquid.Add(chem);
        }

        chem.amount += add;
        currentAmount += add;

        float _fill = Mathf.Clamp(currentAmount / beakerAmount, 0.4f, 1f);

        //fill amount 조절하기
        liquidRender.material.SetFloat("_Fill", _fill);
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

    public void ChangeColor()
    {
        liquidRender.material.SetColor("_SideColor", reactionColor);
        liquidRender.material.SetColor("_TopColor", reactionColor);
    }

    [Header("Pour test")]
    [SerializeField] float pourPerFrame;

    bool isPour = false;

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

    [Header("Chemical Reaction")]
    public Color reactionColor;
    [SerializeField] Vector2 rangeTime;

    [Space2(20), HideField] public bool em1;

    void StartChemicalReaction()
    {
        // 칼륨과 나트륨이 모두 있을 경우에만 화학 반응이 작용한다.
        if (blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Iodine_K)) == null || blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Sulfite_Sodium)) == null) return;

        float t = MapInverse(concentration, rangeTime.x, rangeTime.y);

        Debug.Log($"화학 반응 수행! Time : {t}");

        isReact = true;

        DOVirtual.DelayedCall(t, () =>
        {
            ChangeColor();
        });
    }

    float MapInverse(float c, float t_min, float t_max)
    {
        float epsilon = 0.0001f; // 0으로 나누는 것 방지
        float inv = 1f / (c + epsilon); // 기본 반비례 값
        float invMin = 1f / 1f;         // c = 1일 때 값
        float invMax = 1f / epsilon;    // c ≈ 0일 때 값

        // 0~1로 정규화 후 t_min~t_max로 매핑
        float normalized = (inv - invMin) / (invMax - invMin);
        return t_min + (1f - normalized) * (t_max - t_min);
    }

    void MixBlend(List<ChemInform> mixture)
    {
        Debug.Log(mixture.Count());
        foreach (var m in mixture)
        {
            AddLiquid(m.flag, m.amount);
        }
        CalculateConcentration();
    }

    void CalculateConcentration()
    {
        if (blendedLiquid.Count < 2) return;

        float amount_Distilled = 0f;
        float amount_Other = 0f;

        amount_Distilled = blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Distilled)).amount;

        foreach (var a in blendedLiquid)
        {
            if (a.flag.Equals(ChemFlag.Distilled) || a.flag.Equals(ChemFlag.None)) continue;

            amount_Other += a.amount;
        }

        concentration = amount_Other / (amount_Distilled + amount_Other);
    }

    /*
        Blend가 혼합 되었을 때 완료 되면 호출한다.
        Observer Pattern.
    */

    [SerializeField] private Beaker otherBeaker;

    [SerializeField] private List<ChemInform> testLiquid = new();          //비커에 들어있는 혼합용액 상태.
    [SerializeField] private List<ChemInform> testPowder = new();          //비커에 들어있는 혼합가루 상태.

    void AddSample()
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

    [Button("AddSample"), HideField] public bool btn2;

    // 파티클이 비커의 콜라이더와 충돌할 때 호출
    void OnParticleCollision(GameObject other)
    {
        // 충돌한 파티클 시스템이 '물 파티클'인지 확인하는 로직 추가 가능
        if (other.name.Equals("LiquidPour"))
        {
            //혼합물인지 일반 liquid 인지 체크
            if (other.GetComponentInParent<Beaker>() != null)
            {
                Debug.Log("비커로 부터 나온 Liquid...");
                Beaker b = other.GetComponentInParent<Beaker>();

                b.TransferLiquid(this);

                elapsedPour = 0f;

                return;
            }

            var liquid = other.GetComponentInParent<Liquid>();

            if (liquid != null)
            {
                AddLiquid(liquid.Flag, 0.2f);
            }
        }
    }

    public void TransferLiquid(Beaker target)
    {
        if (currentAmount <= 0) return;

        foreach (var l in blendedLiquid)
        {
            if (l.amount < pourPerFrame)
                target.AddLiquid(l.flag, l.amount);
            else
                target.AddLiquid(l.flag, pourPerFrame);

            l.amount -= pourPerFrame;
            currentAmount -= pourPerFrame;

            liquidRender.material.SetFloat("_Fill", currentAmount / beakerAmount);

            if (l.amount <= 0)
            {
                blendedLiquid.Remove(l);
            }

            if (currentAmount <= 0)
            {
                currentAmount = 0f;
            }
        }
    }
}