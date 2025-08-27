using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class ChemInform
{
    public ChemFlag flag;
    public float amount;

    public ChemInform(ChemFlag flag) { this.flag = flag; amount = 0f; }
    public ChemInform(ChemFlag flag, float amount) { this.flag = flag; this.amount = amount; }
}

public class Beaker : MonoBehaviour
{
    [Header("Beaker Property")]
    [SerializeField] private float beakerAmount = 100f;   // 비커 총 수용량
    [SerializeField, ReadOnly] private float currentAmount;
    [SerializeField] private Renderer liquidRender;

    // (비주얼 초기값 저장용)
    [SerializeField] Color defaultSideColor = Color.white;
    [SerializeField] Color defaultTopColor = Color.white;
    [SerializeField] float defaultFillAtStart = 0f;

    [Header("Blend State")]
    [SerializeField] private List<ChemInform> blendedLiquid = new(); // 액상 성분
    [SerializeField] private List<ChemInform> blendedPowder = new(); // 분말 성분(녹말 등)

    [Header("Concentration (optional)")]
    [SerializeField] float concentration;

    [Header("Pour Angle")]
    [SerializeField] Transform head;
    public float angleThreshold = 120f;

    public List<ChemInform> BlendedLiquid => blendedLiquid;
    public List<ChemInform> BlendedPowder => blendedPowder;

    private PourBehaviour pour;

    // 반응 예약 트윈 핸들
    Tween reactionTween;

    [SerializeField, ReadOnly] float elapsedPour = 0f;   // 외부 유입 없는 시간
    [SerializeField, ReadOnly] bool isReact = false;

    [Header("Pour test")]
    [SerializeField] float pourPerFrame = 1f;   // 프레임당 붓는 양
    private bool isPour = false;

    [Header("Chemical Reaction")]
    public Color reactionColor = Color.blue;

    [Header("Reaction Timing (by dominance)")]
    [SerializeField] float timeWhenPotassiumDominant = 3f; // K 우세
    [SerializeField] float timeWhenSodiumDominant = 1f; // Na 우세
    [SerializeField] float timeWhenSimilar = 2f; // 비슷
    [SerializeField] float dominanceEpsilon = 0.01f;

    // ---- 혼합 세션 추적(방향/녹말 무관하게 동일 결과 보장) ----
    [ReadOnly] public Beaker lastMixPartner;
    [ReadOnly] public float lastMixStamp; // 마지막 혼합 시각

    [Space2(20), HideField] public bool em1;
    [Button("AddSample"), HideField] public bool btn2;

    // 초기 머티리얼 색 스냅샷
    void Awake()
    {
        if (liquidRender && liquidRender.material.HasProperty("_SideColor"))
            defaultSideColor = liquidRender.material.GetColor("_SideColor");
        if (liquidRender && liquidRender.material.HasProperty("_TopColor"))
            defaultTopColor = liquidRender.material.GetColor("_TopColor");
    }

    void Start()
    {
        Reset();
        if (liquidRender && liquidRender.material.HasProperty("_Fill"))
            liquidRender.material.SetFloat("_Fill", GetFill01());

        pour = new PourBehaviour();
        pour.Initialize(head);
    }

    void Update()
    {
        if (isReact) return;

        // 각도 판단하여 붓기 on/off
        PourLiquid();

        if (isPour)
        {
            elapsedPour = 0f;
            lastMixStamp = Time.time;
            PourBlend(); // 실제로 흘려보내는 동작(자체 소모)
            return;
        }

        if(lastMixPartner != null && lastMixPartner.isPour)
        {
            elapsedPour = 0f;
            lastMixStamp = Time.time;
            return;
        }

        // 최근 1초간 외부로부터 파티클 유입이 없으면 반응 판단
        elapsedPour += Time.deltaTime;
        if (elapsedPour > 1f)
        {
            TryStartChemicalReaction();
        }
    }

    float GetFill01() => Mathf.Clamp01(currentAmount / Mathf.Max(1f, beakerAmount));

    public void PourLiquid()
    {
        if (currentAmount <= 0f)
        {
            isPour = false;
            pour.Stop();
            return;
        }

        float angle = Vector3.Angle(head.up, Vector3.up);
        if (angle > angleThreshold)
        {
            isPour = true;
            pour.Start();
        }
        else
        {
            isPour = false;
            pour.Stop();
        }
    }

    // 외부에서 액체 성분 추가
    public void AddLiquid(ChemFlag flag, float add)
    {
        if (flag.Equals(ChemFlag.None) || add <= 0f) return;

        var chem = blendedLiquid.Find(b => b.flag == flag);
        if (chem == null)
        {
            chem = new ChemInform(flag, 0f);
            blendedLiquid.Add(chem);
        }

        chem.amount += add;
        currentAmount += add;

        if (liquidRender && liquidRender.material.HasProperty("_Fill"))
            liquidRender.material.SetFloat("_Fill", GetFill01());
    }

    // 외부에서 분말 성분 추가(녹말 등) — 반응과 무관
    public void AddPowder(ChemFlag flag, float add)
    {
        if (flag.Equals(ChemFlag.None) || add <= 0f) return;

        var chem = blendedPowder.Find(b => b.flag == flag);
        if (chem == null)
        {
            chem = new ChemInform(flag, 0f);
            blendedPowder.Add(chem);
        }
        chem.amount += add;
    }

    public void Reset()
    {
        currentAmount = 0f;
        blendedLiquid.Clear();
        blendedPowder.Clear();

        if (liquidRender && liquidRender.material.HasProperty("_Fill"))
            liquidRender.material.SetFloat("_Fill", 0f);

        isReact = false;
        elapsedPour = 0f;

        lastMixPartner = null;
        lastMixStamp = 0f;
    }

    public void ChangeColor()
    {
        if (!liquidRender) return;
        if (liquidRender.material.HasProperty("_SideColor"))
            liquidRender.material.SetColor("_SideColor", reactionColor);
        if (liquidRender.material.HasProperty("_TopColor"))
            liquidRender.material.SetColor("_TopColor", reactionColor);
    }

    // 이 비커에서 내용물을 밖으로 "쏟아내며 소모" (자체 감소만 처리)
    public void PourBlend()
    {
        if (currentAmount <= 0f) return;

        float consume = Mathf.Min(pourPerFrame, currentAmount);
        currentAmount -= consume;

        // 액상: 모든 성분에서 비율로 감소
        float totalLiquid = blendedLiquid.Sum(x => x.amount);
        if (totalLiquid > 0f)
        {
            float ratio = consume / totalLiquid;
            for (int i = blendedLiquid.Count - 1; i >= 0; i--)
            {
                var b = blendedLiquid[i];
                float dec = b.amount * ratio;
                b.amount -= dec;
                if (b.amount <= 0.0001f) blendedLiquid.RemoveAt(i);
            }
        }

        // (선택) 분말은 모델에 따라 이동/소모 처리 가능 — 결과에는 영향 없음
        float totalPowder = blendedPowder.Sum(x => x.amount);
        if (totalPowder > 0f && totalLiquid > 0f)
        {
            float powderConsume = totalPowder * (consume / (totalLiquid + consume));
            float pRatio = Mathf.Clamp01(powderConsume / Mathf.Max(0.0001f, totalPowder));
            for (int i = blendedPowder.Count - 1; i >= 0; i--)
            {
                var p = blendedPowder[i];
                float dec = p.amount * pRatio;
                p.amount -= dec;
                if (p.amount <= 0.0001f) blendedPowder.RemoveAt(i);
            }
        }

        if (liquidRender && liquidRender.material.HasProperty("_Fill"))
            liquidRender.material.SetFloat("_Fill", GetFill01());
    }

    // ---------- 혼합/반응 로직 ----------
    void TryStartChemicalReaction()
    {
        if (isReact) return;

        // 최소 조건: K/Na가 존재해야 함(녹말 무관)
        if (!HasIodineK() || !HasSodium()) return;

        // 파트너와 섞인 상태라면, 둘 다 정착(1초 무유입 & 비붓기) 후 '합산'으로 판단
        if (lastMixPartner != null)
        {
            var p = lastMixPartner;
            if (p == null) { lastMixPartner = null; return; }     // 파트너 소멸 방어
            if (!IsSettled(this) || !IsSettled(p)) return;        // 정착 대기

            // 합산 총량으로 시간 결정
            float k = GetAmount(this.blendedLiquid, ChemFlag.Iodine_K) +
                       GetAmount(p.blendedLiquid, ChemFlag.Iodine_K);
            float na = GetAmount(this.blendedLiquid, ChemFlag.Sulfite_Sodium) +
                       GetAmount(p.blendedLiquid, ChemFlag.Sulfite_Sodium);
            if (k <= 0f || na <= 0f) return;

            float tPair = DecideByTotals(k, na);
            TriggerReactionForBoth(this, p, tPair);
            return;
        }

        // 파트너가 없는 단독 경로
        float tSolo = GetDominanceDelay();
        if (tSolo < 0f) return;

        isReact = true;
        Debug.Log($"[Solo] Clock reaction! t={tSolo:0.###}s (K/Na dominance)");

        if (ExperimentManager.Instance != null)
            ExperimentManager.Instance.UpdateExperiment(blendedLiquid, tSolo);

        DOVirtual.DelayedCall(tSolo, ChangeColor);
    }

    bool HasIodineK() =>
        blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Iodine_K) && b.amount > 0f) != null;

    bool HasSodium() =>
        blendedLiquid.Find(b => b.flag.Equals(ChemFlag.Sulfite_Sodium) && b.amount > 0f) != null;

    float GetDominanceDelay()
    {
        var k = blendedLiquid.Find(b => b.flag == ChemFlag.Iodine_K);
        var na = blendedLiquid.Find(b => b.flag == ChemFlag.Sulfite_Sodium);

        if (k == null || na == null) return -1f;

        if (k.amount > na.amount + dominanceEpsilon) return timeWhenPotassiumDominant; // K 우세
        if (na.amount > k.amount + dominanceEpsilon) return timeWhenSodiumDominant;   // Na 우세
        return timeWhenSimilar; // 거의 비슷
    }

    // 파티클이 비커의 콜라이더와 충돌할 때 호출(외부에서 유입)
    void OnParticleCollision(GameObject other)
    {
        if (other.name.Equals("LiquidPour"))
        {
            // 다른 비커에서 온 붓기라면 내용물 전달
            var fromBeaker = other.GetComponentInParent<Beaker>();
            if (fromBeaker != null)
            {
                fromBeaker.TransferTo(this);

                // 혼합 파트너/타임스탬프 기록 (방향/녹말 무관 결과 보장)
                this.lastMixPartner = fromBeaker;
                fromBeaker.lastMixPartner = this;

                this.lastMixStamp = fromBeaker.lastMixStamp = Time.time;
                elapsedPour = 0f; // 유입 있었음 → 대기 타이머 리셋
                fromBeaker.elapsedPour = 0f;
                return;
            }

            // 순수 액체 공급기인 경우
            var liquid = other.GetComponentInParent<Liquid>();
            if (liquid != null)
            {
                AddLiquid(liquid.Flag, 0.2f);
                elapsedPour = 0f;
            }
        }
    }

    // 이 비커의 내용물을 대상 비커로 옮김(프레임 단위로 조금씩)
    public void TransferTo(Beaker target)
    {
        if (target == null || currentAmount <= 0f) return;

        float move = Mathf.Min(pourPerFrame, currentAmount);

        // 액상 합계
        float totalLiquid = blendedLiquid.Sum(x => x.amount);
        float lRatio = totalLiquid > 0f ? Mathf.Clamp01(move / totalLiquid) : 0f;

        // 액상 성분 비율 유지하여 전달
        for (int i = blendedLiquid.Count - 1; i >= 0; i--)
        {
            var l = blendedLiquid[i];
            float delta = totalLiquid > 0f ? l.amount * lRatio : 0f;
            if (delta > 0f) target.AddLiquid(l.flag, delta);
            l.amount -= delta;
            if (l.amount <= 0.0001f) blendedLiquid.RemoveAt(i);
        }

        // (선택) 분말도 비율로 이동 — 결과에는 영향 없음
        float totalPowder = blendedPowder.Sum(x => x.amount);
        if (totalPowder > 0f && totalLiquid > 0f)
        {
            float pRatio = lRatio;
            for (int i = blendedPowder.Count - 1; i >= 0; i--)
            {
                var p = blendedPowder[i];
                float delta = p.amount * pRatio;
                if (delta > 0f) target.AddPowder(p.flag, delta);
                p.amount -= delta;
                if (p.amount <= 0.0001f) blendedPowder.RemoveAt(i);
            }
        }

        // 총량 감소 & 표시 갱신
        currentAmount -= move;
        if (currentAmount < 0f) currentAmount = 0f;

        if (liquidRender && liquidRender.material.HasProperty("_Fill"))
            liquidRender.material.SetFloat("_Fill", GetFill01());

        this.elapsedPour = 0f;
        this.lastMixStamp = Time.time;
    }

    // ---- Helpers ----
    bool IsSettled(Beaker b) => b != null && !b.isPour && (Time.time - b.lastMixStamp) > 1f;

    float GetAmount(List<ChemInform> list, ChemFlag flag)
    {
        var x = list.Find(b => b.flag == flag);
        return x != null ? x.amount : 0f;
    }

    float DecideByTotals(float totalK, float totalNa)
    {
        if (totalK > totalNa + dominanceEpsilon) return timeWhenPotassiumDominant;
        if (totalNa > totalK + dominanceEpsilon) return timeWhenSodiumDominant;
        return timeWhenSimilar;
    }

    void TriggerReactionForBoth(Beaker a, Beaker b, float t)
    {
        if (a.isReact && b.isReact) return;

        a.isReact = true;
        b.isReact = true;

        Debug.Log($"[Pair] Clock reaction! t={t:0.###}s (sum of both)");

        // 보고서는 합산 상태로 한 번만 기록
        if (ExperimentManager.Instance != null)
        {
            var combined = BuildCombinedLiquids(a, b);
            ExperimentManager.Instance.UpdateExperiment(combined, t);
        }

        DOVirtual.DelayedCall(t, () => { a.ChangeColor(); b.ChangeColor(); });

        // 세션 종료
        a.lastMixPartner = null;
        b.lastMixPartner = null;
    }

    List<ChemInform> BuildCombinedLiquids(Beaker a, Beaker b)
    {
        var dict = new Dictionary<ChemFlag, float>();

        void Accum(List<ChemInform> src)
        {
            foreach (var ci in src)
            {
                if (ci == null || ci.flag == ChemFlag.None) continue;
                if (!dict.ContainsKey(ci.flag)) dict[ci.flag] = 0f;
                dict[ci.flag] += ci.amount;
            }
        }

        Accum(a.blendedLiquid);
        Accum(b.blendedLiquid);

        var list = new List<ChemInform>();
        foreach (var kv in dict)
            list.Add(new ChemInform(kv.Key, kv.Value));

        return list;
    }

    // 색 변경 예약은 이걸로만 호출하도록(나중에 리셋 시 안전 종료)
    void ScheduleReaction(float t)
    {
        KillReactionTween();
        reactionTween = DOVirtual.DelayedCall(t, ChangeColor);
    }
    void KillReactionTween()
    {
        if (reactionTween != null && reactionTween.IsActive()) reactionTween.Kill();
        reactionTween = null;
    }

    // 소프트리셋 -> 시계반응 두번할수 있도록 조정
    public void ResetForNextRun()
    {
        // 예약/진행 중 트윈 중단
        KillReactionTween();

        // 붓기/혼합 상태 초기화
        isPour = false;
        pour?.Stop();
        lastMixPartner = null;
        lastMixStamp = 0f;

        // 성분/양 초기화
        currentAmount = 0f;
        blendedLiquid.Clear();
        blendedPowder.Clear();

        // 타이머/플래그 초기화
        isReact = false;
        elapsedPour = 0f;

        // 비주얼 초기화
        if (liquidRender)
        {
            if (liquidRender.material.HasProperty("_SideColor"))
                liquidRender.material.SetColor("_SideColor", defaultSideColor);
            if (liquidRender.material.HasProperty("_TopColor"))
                liquidRender.material.SetColor("_TopColor", defaultTopColor);
            if (liquidRender.material.HasProperty("_Fill"))
                liquidRender.material.SetFloat("_Fill", defaultFillAtStart);
        }
    }
}
