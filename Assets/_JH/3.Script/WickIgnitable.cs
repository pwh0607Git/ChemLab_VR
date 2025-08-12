using UnityEngine;
using UnityEngine.Events;

public class WickIgnitable : MonoBehaviour
{
    [Header("심지 불 파티클")]
    public ParticleSystem wickFlameFX;

    [Header("심지 배치 플래그(소켓에 꽂혔는지")]
    public WickPlacedFlag placedFlag;

    public bool IsIgnited { get; private set; } = false;

    public UnityEvent onIgnited;

    public bool TryIgnite()
    {
        if (IsIgnited) return false;
        if (placedFlag != null && !placedFlag.IsWickPlaced) return false;

        IsIgnited = true;
        if (wickFlameFX && !wickFlameFX.isPlaying) wickFlameFX.Play();
        onIgnited?.Invoke();
        Debug.Log("[wick] 점화");
        return true;
    }
}
