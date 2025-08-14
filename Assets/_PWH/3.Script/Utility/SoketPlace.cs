using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class SoketPlace : MonoBehaviour
{
    public bool IsPlaced { get; protected set; } = false;

    public virtual void OnObjectSelected(SelectEnterEventArgs _)
    {
        IsPlaced = true;
        Debug.Log("[Wick] 물체 배치");
    }

    public virtual void OnObjectDeselected(SelectExitEventArgs _)
    {
        IsPlaced = false;
        Debug.Log("[Wick] 물체 제거");
    }
}
