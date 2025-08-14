using UnityEngine;
using UnityEngine.InputSystem;

public class TorchFlameController : MonoBehaviour
{
    [Header("VFX Manager 설정")]
    public VFXFlag torchFlameFlag = VFXFlag.FlameFx2;  // 매니저에 등록된 토치 불  VFX 플래그
    public Transform vfxAnchor; // 불 위치( 없으면 이 오브젝트)
    public bool loopFlame = true; // 토치 불은 루프

    [Header("XR 입력 (옵션)")]
    public InputActionReference toggleFlameAction;

    public bool IsOn { get; private set; } = false;

    // 스폰된 불 VFX 인스턴스
    private VFX _flameVfx;

    private void OnEnable()
    {
        if(toggleFlameAction != null)
        {
            toggleFlameAction.action.performed += OnTogglePerformed;
            toggleFlameAction.action.Enable();
        }
    }
    private void OnDisable()
    {
        if (toggleFlameAction != null)
        {
            toggleFlameAction.action.performed -= OnTogglePerformed;
            toggleFlameAction.action.Disable();
        }

        // 꺼질 때 정리
        if (_flameVfx != null)
        {
            _flameVfx.Stop();
            _flameVfx = null;
        }
        IsOn = false;
    }

    void OnTogglePerformed(InputAction.CallbackContext _)
    {
        Toggle();
    }

    public void Toggle()
    {
        SetFlame(!IsOn);
    }

    public void SetFlame(bool on)
    {
        if (on == IsOn) return;
        IsOn = on;

        if (on)
        {
            // 켜기 :스폰 (이미 있으면 재생)
            if (_flameVfx == null)
            {
                var anchor = vfxAnchor != null ? vfxAnchor : transform;
                _flameVfx = VFXManager.Instance.SpawnVFX(
                    torchFlameFlag,
                    Vector3.zero,
                    anchor.rotation,
                    anchor, // 토치에 붙여서 이동
                    loopFlame
                    );
                if (_flameVfx == null)
                {
                    Debug.LogWarning("[토치] 불 VFX가 스폰되지 않았습니다");
                    IsOn = false;
                    return;
                }
            }
            else
            {
                _flameVfx.Play();
            }
        }
        else
        {
            if(_flameVfx != null)
            {
                _flameVfx.Stop();
                _flameVfx = null;
            }
        }
    }
}
