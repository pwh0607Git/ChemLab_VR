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
        chemInforms.Add(new ChemInform(flag, amount));
    }
}

// 중크롬산 암모늄 실험 (향후 확장)
[Serializable]
public class ExplosionReactionCase { }

public class ExperimentManager : BehaviourSingleton<ExperimentManager>
{
    protected override bool IsDontDestroy() => false;

    [Header("Clock Reaction")]
    [SerializeField, ReadOnly] private List<ClockReactionCase> cachedData_ClockReaction = new(); // ✅ 선언부에서 즉시 초기화
    public List<ClockReactionCase> CachedData_ClockReaction => cachedData_ClockReaction;

    [Header("Explosion Reaction")]
    public string res;

    private void Awake()
    {
        // ✅ 혹시라도 null이면 복구
        if (cachedData_ClockReaction == null)
            cachedData_ClockReaction = new List<ClockReactionCase>();
    }

    private void OnValidate()
    {
        // ✅ 에디터에서 Serialize 유지하며 null 방지
        if (cachedData_ClockReaction == null)
            cachedData_ClockReaction = new List<ClockReactionCase>();
    }

    /// <summary>
    /// 실험 기록 업데이트 (Beaker에서 호출)
    /// </summary>
    public void UpdateExperiment(List<ChemInform> informs, float t1)
    {
        if (informs == null || informs.Count == 0) return;

        // ✅ 리스트가 혹시라도 비어있으면 다시 보호
        if (cachedData_ClockReaction == null)
            cachedData_ClockReaction = new List<ClockReactionCase>();

        // ✅ 깊은 복사 형태로 케이스 구성
        var case1 = new ClockReactionCase();
        foreach (var i in informs)
        {
            if (i == null || i.flag == ChemFlag.None) continue;
            case1.AddData(i.flag, i.amount);
        }

        case1.reactionTime = Mathf.Max(0f, t1);
        cachedData_ClockReaction.Add(case1);
    }

    /// <summary>
    /// 최근 기록 반환(리포트/디버그용)
    /// </summary>
    public ClockReactionCase GetLastClockCase()
    {
        if (cachedData_ClockReaction == null || cachedData_ClockReaction.Count == 0)
            return null;
        return cachedData_ClockReaction[cachedData_ClockReaction.Count - 1];
    }

    /// <summary>
    /// 모든 기록 초기화
    /// </summary>
    public void ClearClockCache()
    {
        if (cachedData_ClockReaction == null)
            cachedData_ClockReaction = new List<ClockReactionCase>();
        else
            cachedData_ClockReaction.Clear();
    }
}
