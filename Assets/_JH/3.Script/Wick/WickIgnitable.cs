using UnityEngine;
using UnityEngine.Events;

public class WickIgnitable : MonoBehaviour
{
    [Header("심지 배치 플래그(소켓에 꽂혔는지")]
    public WickPlacedFlag placedFlag;

    [Header("VFX Manager 사용 설정")]
    public VFXFlag wickFlameFlag = VFXFlag.FlameFx;
    public Transform vfxAnchor;
    public bool loopFlame = true;

    public bool IsIgnited { get; private set; } = false;
    public UnityEvent onIgnited;

    // 스폰된 VFX 인스턴스 보관
    private VFX _wickFlameVfx;
    public bool TryIgnite()
    {
        if (IsIgnited) return false;
        if (placedFlag != null && !placedFlag.IsWickPlaced) return false;

        // VFX 스폰
        SpawnVFX();

        IsIgnited = true;
        onIgnited?.Invoke();
        Debug.Log("[wick] 점화");
        return true;
    }
    void SpawnVFX()
    {
        var anchor = vfxAnchor != null ? vfxAnchor : transform;
        _wickFlameVfx = VFXManager.Instance.SpawnVFX(
            wickFlameFlag,
            Vector3.zero,
            anchor.rotation,
            anchor, // 심지에 붙여서 같이 움직이도록
            loopFlame
            );

        if (_wickFlameVfx == null)
            Debug.LogWarning("[WickIgnitable] wickFlame VFX 스폰 실패");
    }

    //불 끄기
    public void Extinguish()
    {
        if (!IsIgnited) return;
        IsIgnited = false;

        if(_wickFlameVfx != null)
        {
            _wickFlameVfx.Stop();
            _wickFlameVfx = null;
        }
    }
}
