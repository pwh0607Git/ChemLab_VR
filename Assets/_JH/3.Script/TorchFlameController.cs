using UnityEngine;
using UnityEngine.InputSystem;

public class TorchFlameController : MonoBehaviour
{
    [Header("토치 불 파티클")]
    public ParticleSystem flameFX;

    [Header("XR 입력 (옵션)")]
    public InputActionReference toggleFlameAction;

    public bool IsOn { get; private set; } = false;

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
            toggleFlameAction.action.Enable();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            Toggle();
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
        IsOn = on;
        if (!flameFX) return;
        if (on && !flameFX.isPlaying) flameFX.Play();
        if(!on && !flameFX.isPlaying) flameFX.Stop();
    }

}
