// SoftResetHotkey.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class SoftResetHotkey : MonoBehaviour
{
    [Header("Target")]
    public ClockResetManager resetManager;             // SoftResetClockOnly()가 있는 매니저

    [Header("Input (Input System)")]
    // XRI Default Input Actions → XRI LeftHand Interaction(또는 네가 만든 액션맵) → primaryButton 액션을 드래그
    public InputActionReference primaryButtonAction;

    [Header("Optional")]
    public float cooldown = 0.3f;                      // 연타 방지
    private float _next;

    private void OnEnable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed += OnPressed;
            primaryButtonAction.action.Enable();       // 이 오브젝트가 켜질 때 액션 활성화
        }
    }

    private void OnDisable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed -= OnPressed;
            primaryButtonAction.action.Disable();
        }
    }

    private void OnPressed(InputAction.CallbackContext ctx)
    {
        if (Time.time < _next) return;
        _next = Time.time + cooldown;

        if (resetManager != null)
        {
            resetManager.SoftResetClockOnly();
            Debug.Log("[SoftResetHotkey] SoftResetClockOnly fired by Left PrimaryButton.");
        }
        else
        {
            Debug.LogWarning("[SoftResetHotkey] resetManager is not assigned.");
        }
    }
}
