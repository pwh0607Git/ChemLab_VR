using UnityEngine;
using UnityEngine.InputSystem;

public class SoftResetHotkey : MonoBehaviour
{
    [Header("Clock reaction ONLY")]
    public ClockResetManager resetManager;   // (비커 내용 리셋)

    [Header("Scene-wide Reset (옵션)")]
    public SceneSoftResetAll sceneResetAll;  // (자리/포즈 리셋)

    [Header("Input (Input System)")]
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
        // 1) 비커 내용(시계반응만) 리셋
        if (resetManager != null)
            resetManager.SoftResetClockOnly();   // ← 기존 동작 유지

        // 2) 씬 내 모든 ISoftResettable(숟가락/약품통/트레이 등) 자리 리셋
        if (sceneResetAll != null)
            sceneResetAll.SoftResetAll();
        else
            Debug.LogWarning("[SoftResetHotkey] SceneSoftResetAll reference is missing.");
    }
}
