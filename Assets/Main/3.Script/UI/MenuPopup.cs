using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
#if XR_CORE_UTILS_EXISTS // CoreUtils가 있을 때만
using Unity.XR.CoreUtils;
#endif


public class MenuPopup : MonoBehaviour
{
    [SerializeField] Transform xrCamera;
    public float popupDistance = 2.0f;
    public GameObject popupPanel;
    public GameObject exitPopupPanel; // PopUP_Image 오브젝트

    // UI
    public TMP_Text txtSliderLabel;
    public Slider targetSlider;
    public TMP_Text txtContinue;
    public TMP_Text txtExit;

    // Exit Popup용 텍스트
    public TMP_Text exitPopupContinueText; // PopUP_Image의 "계속"
    public TMP_Text exitPopupEndGameText;  // PopUP_Image의 "게임종료"

    public Color highlightColor = Color.green, normalColor = Color.white;
    public Color sliderHighlightColor = Color.yellow;

    public InputActionReference stickInput;      // XR 오른쪽 스틱
    public InputActionReference triggerClickR;   // 트리거
    public InputActionReference btnMenu;

    // 메뉴 선택 인덱스
    enum MenuItem { Slider, Continue, Exit }
    MenuItem currentSelection = MenuItem.Slider;

    // Exit Popup 선택 인덱스
    enum ExitPopupMenu { Continue, Exit }
    ExitPopupMenu exitPopupSelection = ExitPopupMenu.Continue;

    // 입력 딜레이
    float inputBufferTime = 0.25f, lastInputTime = 0f;
    float exitPopupInputBufferTime = 0.25f, exitPopupLastInputTime = 0f;

    // 슬라이더 감도
    public float sliderSensitivity = 0.7f;

    bool isPaused = false;
    bool isExitPopupActive = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // 씬이 바뀔 때마다 자동으로 XR카메라를 다시 찾도록 이벤트 연결
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 씬 이벤트 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 전환 시마다 호출됨
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 새 씬에서 XR카메라(혹은 Main Camera) 찾기
        xrCamera = FindXRCamera();
    }

    // XR카메라를 자동으로 찾아주는 함수
    Transform FindXRCamera()
    {
#if XR_CORE_UTILS_EXISTS
        var xrOrigin = GameObject.FindObjectOfType<XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
            return xrOrigin.Camera.transform;
#endif

        var mainCam = Camera.main;
        if (mainCam != null)
            return mainCam.transform;
        return null;
    }

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

        // ----------- 1. 팝업이 켜진 경우 ----------
        if (isExitPopupActive)
        {
            Vector2 axis = stickInput.action.ReadValue<Vector2>();
            float now = Time.unscaledTime;

            // 좌/우 입력으로 커서 이동 (좌 = 계속, 우 = 게임종료)
            if (Mathf.Abs(axis.x) > 0.6f && now - exitPopupLastInputTime > exitPopupInputBufferTime)
            {
                int dir = (axis.x > 0) ? 1 : -1;
                int idx = (int)exitPopupSelection + dir;
                idx = Mathf.Clamp(idx, 0, 1); // 0~1
                exitPopupSelection = (ExitPopupMenu)idx;
                exitPopupLastInputTime = now;
                UpdateExitPopupHighlight();
            }
            return; // 팝업이 켜진 동안에는 아래 코드 실행 안 함!
        }

        // ----------- 2. 메인 메뉴 입력 ----------
        Vector2 axisMenu = stickInput.action.ReadValue<Vector2>();
        float nowMenu = Time.unscaledTime;

        // 위/아래 입력 처리 (선택 이동)
        if (Mathf.Abs(axisMenu.y) > 0.6f && nowMenu - lastInputTime > inputBufferTime)
        {
            int dir = (axisMenu.y > 0) ? -1 : 1;
            int idx = (int)currentSelection + dir;
            idx = Mathf.Clamp(idx, 0, 2); // 0~2
            currentSelection = (MenuItem)idx;
            lastInputTime = nowMenu;
            UpdateHighlight();
        }

        // 슬라이더 조정 (슬라이더 선택 중)
        if (currentSelection == MenuItem.Slider && Mathf.Abs(axisMenu.x) > 0.2f)
        {
            float newValue = targetSlider.value + axisMenu.x * sliderSensitivity * Time.unscaledDeltaTime;
            targetSlider.value = Mathf.Clamp01(newValue);
        }
    }

    void UpdateHighlight()
    {
        // 하이라이트 갱신 (메인 메뉴)
        txtSliderLabel.color = (currentSelection == MenuItem.Slider) ? sliderHighlightColor : normalColor;
        txtContinue.color = (currentSelection == MenuItem.Continue) ? highlightColor : normalColor;
        txtExit.color = (currentSelection == MenuItem.Exit) ? highlightColor : normalColor;

        // 슬라이더 핸들 색상도 바꿔줄 수 있음 (Image 필요)
        var handle = targetSlider.handleRect?.GetComponent<Image>();
        if (handle != null)
            handle.color = (currentSelection == MenuItem.Slider) ? sliderHighlightColor : normalColor;
    }

    void UpdateExitPopupHighlight()
    {
        // 팝업 내 텍스트 하이라이트
        if (exitPopupContinueText != null)
            exitPopupContinueText.color = (exitPopupSelection == ExitPopupMenu.Continue) ? highlightColor : normalColor;
        if (exitPopupEndGameText != null)
            exitPopupEndGameText.color = (exitPopupSelection == ExitPopupMenu.Exit) ? highlightColor : normalColor;
    }

    void OnTriggerClick(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        // ----- 팝업이 켜진 경우 -----
        if (isExitPopupActive)
        {
            switch (exitPopupSelection)
            {
                case ExitPopupMenu.Continue:
                    CloseExitPopup();
                    break;
                case ExitPopupMenu.Exit:
                    ExitGame();
                    break;
            }
            return;
        }

        // ----- 메인 메뉴 트리거 처리 -----
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
                Debug.Log("끝내기! (팝업오픈)");
                OpenExitPopup();
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
        MoveToCameraFront();
        Time.timeScale = 0;
        isPaused = true;
        currentSelection = MenuItem.Slider;
        UpdateHighlight();
    }
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;

        // === 팝업이 켜져있으면 강제 Off & 상태 초기화 ===
        if (isExitPopupActive)
        {
            exitPopupPanel.SetActive(false);
            isExitPopupActive = false;
        }
        // 메인 메뉴 커서도 리셋(원한다면)
        currentSelection = MenuItem.Slider;
        UpdateHighlight();
    }

    // "게임종료" 버튼에서 연결
    public void OpenExitPopup()
    {
        exitPopupPanel.SetActive(true);
        isExitPopupActive = true;
        exitPopupSelection = ExitPopupMenu.Continue; // 무조건 첫 번째로
        UpdateExitPopupHighlight();
    }

    // 팝업 "계속" 버튼에서 연결
    public void CloseExitPopup()
    {
        exitPopupPanel.SetActive(false);
        isExitPopupActive = false;
    }

    // 팝업 "게임종료" 버튼에서 연결
    public void ExitGame()
    {
        Debug.Log("게임 종료합니다");
        // Application.Quit();
    }
    void MoveToCameraFront()
    {
        if (xrCamera == null) xrCamera = Camera.main.transform;
        popupPanel.transform.position = xrCamera.position + xrCamera.forward * popupDistance;
        popupPanel.transform.rotation = Quaternion.LookRotation(xrCamera.forward, xrCamera.up);
        popupPanel.transform.position = xrCamera.position + xrCamera.forward * popupDistance;
        popupPanel.transform.rotation = Quaternion.LookRotation(xrCamera.forward, xrCamera.up);
    }
}
