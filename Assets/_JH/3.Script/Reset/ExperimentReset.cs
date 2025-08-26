using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class ExperimentResetHard : MonoBehaviour
{
    [Header("트리거 입력 (둘 중 하나)")]
    [SerializeField] private InputActionReference resetAction;     // 비워두면 폴링 사용
    [SerializeField] private bool useLeftSecondaryPolling = true;  // 왼손 Y
    [SerializeField] private bool useRightSecondaryPolling = false;// 오른손 B

    [Header("어떤 씬을 다시 로드할지")]
    [SerializeField] private bool reloadFirstLoaded = false;       // true=앱 처음 로드된 씬
    [SerializeField] private string sceneName = "";                // 지정 이름(옵션, 비우면 무시)

    private static string s_firstScene;
    private bool _prev;

    private void Awake()
    {
        if (string.IsNullOrEmpty(s_firstScene))
            s_firstScene = SceneManager.GetActiveScene().name;
    }

    private void OnEnable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.performed += OnReset;
            resetAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.performed -= OnReset;
            resetAction.action.Disable();
        }
    }

    private void Update()
    {
        // 인풋 액션을 쓰면 폴링 생략
        if (resetAction != null && resetAction.action != null) return;

        bool pressed = false;
        if (useLeftSecondaryPolling)
        {
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (left.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v) pressed = true;
        }
        else if (useRightSecondaryPolling)
        {
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (right.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v) pressed = true;
        }

        if (pressed && !_prev) HardReset();
        _prev = pressed;
    }

    private void OnReset(InputAction.CallbackContext _)
    {
        HardReset();
    }

    public void HardReset()
    {
        Time.timeScale = 1f; // 혹시 멈춰있으면 원복

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (reloadFirstLoaded)
            SceneManager.LoadScene(s_firstScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
