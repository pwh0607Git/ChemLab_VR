using UnityEngine;
using UnityEngine.InputSystem;

public class SoftResetHotkey : MonoBehaviour
{
    [Header("Clock reaction ONLY")]
    public ClockResetManager resetManager;   // 시계반응 리셋 매니저만 연결

    [Header("Input (Input System)")]
    // XRI Left/Right Interaction 액션맵의 primaryButton 같은 액션을 연결
    public InputActionReference primaryButtonAction;

    [Header("Optional")]
    public float cooldown = 0.3f;
    float _next;

    void OnEnable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed += OnPressed;
            primaryButtonAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.performed -= OnPressed;
            primaryButtonAction.action.Disable();
        }
    }

    public void Trigger() => DoReset(); // UI 버튼에서도 호출 가능

    void OnPressed(InputAction.CallbackContext _)
    {
        if (Time.time < _next) return;
        _next = Time.time + cooldown;
        DoReset();
    }

    void DoReset()
    {
        if (resetManager != null)
        {
            resetManager.SoftResetClockOnly(); // ✅ 시계반응만 소프트리셋
        }
        else
        {
            Debug.LogWarning("[SoftResetHotkey] ClockResetManager reference is missing.");
        }
    }
}
