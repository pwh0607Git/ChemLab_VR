using CustomInspector;
using TMPro;
using UnityEngine;

public class ClockReactionReport : EPReport
{
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;
    [SerializeField] TextMeshProUGUI reactionTime1;
    
    [SerializeField] TextMeshProUGUI text5;
    [SerializeField] TextMeshProUGUI text6;
    [SerializeField] TextMeshProUGUI text7;
    [SerializeField] TextMeshProUGUI reactionTime2;

    public void UpdateReport(float amount1, float amount2, float amount3, float reactionTime1
    ,float amount4, float amount5, float amount6, float reactionTime2)
    {
        text1.text = amount1.ToString();
        text2.text = amount2.ToString();
        text3.text = amount3.ToString();
        this.reactionTime1.text = reactionTime1.ToString();
        
        text5.text = amount4.ToString();
        text6.text = amount5.ToString();
        text7.text = amount6.ToString();
        this.reactionTime2.text = reactionTime2.ToString();
    }

    [Button("TestReporting"), HideField] public bool testButton1;

    public void TestReporting()
    {
        UpdateReport(10, 10, 10, 1f, 20, 12, 13, 5f);
    }
}