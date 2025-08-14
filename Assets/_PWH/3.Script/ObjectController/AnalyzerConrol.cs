using System.Collections.Generic;
using CustomInspector;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AnalyzerConrol : SoketPlace
{
    [SerializeField, ReadOnly] private Beaker currentBeaker;
    [SerializeField] float analyzeTime;

    void StartAnalyze(Beaker b)
    {
        Debug.Log("분석 시작...");
        currentBeaker = b;

        var liquidCom = b.BlendedLiquid;
        var liquidPowder = b.BlendedPowder;

        Sequence aSeq = DOTween.Sequence();

        aSeq.AppendInterval(0.3f)
            .AppendCallback(() => b.SetGrabable(false))
            .AppendInterval(analyzeTime)
            .AppendCallback(() =>
            {
                ShowUI(liquidCom, liquidPowder);
            }).OnComplete(() => b.SetGrabable(true));
    }

    [Header("UI")]
    [SerializeField] TextMeshProUGUI result;

    void ShowUI(List<ChemInform> liquid, List<ChemInform> powder)
    {
        string l_str = "";
        string p_str = "";

        foreach (var l in liquid)
        {
            l_str += $"{l.flag.ToString()} : {l.amount} L\n";
        }

        foreach (var p in powder)
        {
            p_str += $"{p.flag.ToString()} : {p.amount} g\n";
        }

        result.text = l_str + p_str;
    }

    void ResetResultUI()
    {
        // result.text = "";
    }

    public override void OnObjectSelected(SelectEnterEventArgs _)
    {
        Beaker b = _.interactableObject.transform.gameObject.GetComponent<Beaker>();

        if (b == null) return;

        IsPlaced = true;
        StartAnalyze(b);
        Debug.Log("[Analyzer] 물체 배치");
    }

    public override void OnObjectDeselected(SelectExitEventArgs _)
    {
        IsPlaced = false;
        Beaker b = _.interactableObject.transform.gameObject.GetComponent<Beaker>();

        Debug.Log("[Analyzer] 물체 제거");
        ResetResultUI();
    }
}