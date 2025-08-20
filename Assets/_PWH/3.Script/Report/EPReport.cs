using TMPro;
using UnityEngine;

public enum EPType
{
    ClockReaction,
    
    Explosion,
}


public abstract class EPReport : MonoBehaviour
{
    [SerializeField] EPType type;
    public EPType Type => type;

    [SerializeField] string ep_Result;
    [SerializeField] TextMeshProUGUI resultContent;

    void Start()
    {
        resultContent.text = "";
    }

    public virtual void WriteResult()
    {
        resultContent.text = ep_Result;
    }
}