using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class MenuPopup : MonoBehaviour
{
    [SerializeField] Transform xrCamera;
    public float popupDistance = 2.0f;
    public GameObject popupPanel;

    // UI
    public TMP_Text txtSliderLabel;
    public Slider targetSlider;
    public TMP_Text txtContinue;
    public TMP_Text txtExit;

    public Color highlightColor = Color.green, normalColor = Color.white;
    public Color sliderHighlightColor = Color.yellow;

    public InputActionReference stickInput;      // XR 오른쪽 스틱
    public InputActionReference triggerClickR;   // 트리거
    public InputActionReference btnMenu;

    // 메뉴 선택 인덱스
    enum MenuItem { Slider, Continue, Exit }
    MenuItem currentSelection = MenuItem.Slider;

    // 입력 딜레이
    float inputBufferTime = 0.25f, lastInputTime = 0f;

    // 슬라이더 감도
    public float sliderSensitivity = 0.7f;

    bool isPaused = false;

    void OnEnable()
    {
        stickInput?.action.Enable();
        triggerClickR?.action.Enable();
        btnMenu?.action.Enable();

        triggerClickR.action.performed += OnTriggerClick;
        btnMenu.action.performed += OnMenuPressed;
    }
    void OnDisable()
    {
        triggerClickR.action.performed -= OnTriggerClick;
        btnMenu.action.performed -= OnMenuPressed;

        stickInput?.action.Disable();
        triggerClickR?.action.Disable();
        btnMenu?.action.Disable();
    }

    void Update()
    {
        if (!isPaused) return;

        Vector2 axis = stickInput.action.ReadValue<Vector2>();
        float now = Time.unscaledTime;

        // 위/아래 입력 처리 (선택 이동)
        if (Mathf.Abs(axis.y) > 0.6f && now - lastInputTime > inputBufferTime)
        {
            int dir = (axis.y > 0) ? -1 : 1;
            int idx = (int)currentSelection + dir;
            idx = Mathf.Clamp(idx, 0, 2); // 0~2
            currentSelection = (MenuItem)idx;
            lastInputTime = now;
            UpdateHighlight();
        }

        // 슬라이더 조정 (슬라이더 선택 중)
        if (currentSelection == MenuItem.Slider && Mathf.Abs(axis.x) > 0.2f)
        {
            float newValue = targetSlider.value + axis.x * sliderSensitivity * Time.unscaledDeltaTime;
            targetSlider.value = Mathf.Clamp01(newValue);
        }
    }

    void UpdateHighlight()
    {
        // 하이라이트 갱신
        txtSliderLabel.color = (currentSelection == MenuItem.Slider) ? sliderHighlightColor : normalColor;
        txtContinue.color = (currentSelection == MenuItem.Continue) ? highlightColor : normalColor;
        txtExit.color = (currentSelection == MenuItem.Exit) ? highlightColor : normalColor;

        // 슬라이더 핸들 색상도 바꿔줄 수 있음 (Image 필요)
        var handle = targetSlider.handleRect?.GetComponent<Image>();
        if (handle != null)
            handle.color = (currentSelection == MenuItem.Slider) ? sliderHighlightColor : normalColor;
    }

    void OnTriggerClick(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        switch (currentSelection)
        {
            case MenuItem.Slider:
                // 필요하면 효과음 등만
                break;
            case MenuItem.Continue:
                Debug.Log("계속!");
                ClosePopup();
                break;
            case MenuItem.Exit:
                Debug.Log("끝내기!");
                Application.Quit();
                break;
        }
    }
    void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        if (isPaused) ClosePopup();
        else OpenPopup();
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        MoveToCameraFront();   // ← 여기!
        Time.timeScale = 0;
        isPaused = true;
        UpdateHighlight();
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
    void MoveToCameraFront()
    {
        if (xrCamera == null) xrCamera = Camera.main.transform;
        popupPanel.transform.position = xrCamera.position + xrCamera.forward * popupDistance;
        popupPanel.transform.rotation = Quaternion.LookRotation(xrCamera.forward, xrCamera.up);
    }



}
