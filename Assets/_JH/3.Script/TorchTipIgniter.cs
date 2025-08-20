using UnityEngine;

public class TorchTipIgniter : MonoBehaviour
{
    public TorchFlameController torch;
    public LayerMask wickLayer;

    private void OnTriggerStay(Collider other)
    {
        if (torch == null || !torch.IsOn) return; // 불이 켜져 있어야만
        if (((1 << other.gameObject.layer) & wickLayer) == 0) return;

        var ignitable = other.GetComponentInParent<WickIgnitable>();
        if(ignitable != null)
        {
            ignitable.TryIgnite();
        }
    }
}
