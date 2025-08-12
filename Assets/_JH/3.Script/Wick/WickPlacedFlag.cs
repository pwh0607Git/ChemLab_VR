using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WickPlacedFlag : MonoBehaviour
{
    public bool IsWickPlaced { get; private set; } = false;

    public void OnWickSelected(SelectEnterEventArgs _)
    {
        IsWickPlaced = true;
        Debug.Log("[Wick] 심지 배치 완료");
    }

    public void OnWickDeselected(SelectExitEventArgs _)
    {
        IsWickPlaced = false;
        Debug.Log("[Wick] 심지 제거됨");
    }

}
