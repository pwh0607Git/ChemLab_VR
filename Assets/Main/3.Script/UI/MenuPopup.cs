using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
#if XR_CORE_UTILS_EXISTS
using Unity.XR.CoreUtils;
#endif

public class MenuPopup : MonoBehaviour
{
    [SerializeField] Transform xrCamera;
    public float popupDistance = 2.0f;
    public GameObject popupPanel;
    public GameObject exitPopupPanel;
    public GameObject soundPopupPanel;

    // ------------------ [사운드팝업(음량조절)] ------------------
    [Header("사운드팝업 - 텍스트/슬라이더")]
    public TMP_Text masterLabel;       // "전체 음량"
    public TMP_Text bgmLabel;          // "배경 음악"
    public TMP_Text sfxLabel;          // "효과음"
    public TMP_Text closeText;         // "닫기"
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    // ------------------ [기존 메뉴팝업] ------------------
    [Header("메인팝업 - 텍스트")]
    public TMP_Text txtSound;          // "음량 조절"
    public TMP_Text txtContinue;       // "게임 계속"
    public TMP_Text txtExit;           // "게임 종료"

    // ------------------ [게임종료팝업] ------------------
    [Header("게임종료팝업 - 텍스트")]
    public TMP_Text exitPopupContinueText;
    public TMP_Text exitPopupEndGameText;

    // ------------------ [인풋/컬러] ------------------
    [Header("인풋 & 컬러")]
    public InputActionReference stickInput;
    public InputActionReference triggerClickR;
    public InputActionReference btnMenu;
    public Color highlightColor = Color.green;
    public Color normalColor = Color.white;
    public Color sliderHighlightColor = Color.yellow;

    // ---------- 메뉴 선택 인덱스 ----------
    enum MenuItem { Sound, Continue, Exit }
    MenuItem currentSelection = MenuItem.Sound;

    // ---------- 사운드팝업 선택 인덱스 ----------
    enum SoundPopupItem { Master, BGM, SFX, Close }
    int soundPopupSelection = 0; // 0~3

    // ---------- 게임종료 팝업 ----------
    enum ExitPopupMenu { Continue, Exit }
    ExitPopupMenu exitPopupSelection = ExitPopupMenu.Continue;

    // ---------- 입력 관련 ----------
    float inputBufferTime = 0.25f, lastInputTime = 0f;
    float exitPopupInputBufferTime = 0.25f, exitPopupLastInputTime = 0f;
    float soundPopupInputBufferTime = 0.25f, soundPopupLastInputTime = 0f;
    public float sliderSensitivity = 0.7f;

    bool isPaused = false;
    bool isExitPopupActive = false;
    bool isSoundPopupActive = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        xrCamera = FindXRCamera();
    }
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

        // 1. 게임종료 팝업
        if (isExitPopupActive)
        {
            Vector2 axis = stickInput.action.ReadValue<Vector2>();
            float now = Time.unscaledTime;
            if (Mathf.Abs(axis.x) > 0.6f && now - exitPopupLastInputTime > exitPopupInputBufferTime)
            {
                int dir = (axis.x > 0) ? 1 : -1;
                int idx = (int)exitPopupSelection + dir;
                idx = Mathf.Clamp(idx, 0, 1);
                exitPopupSelection = (ExitPopupMenu)idx;
                exitPopupLastInputTime = now;
                UpdateExitPopupHighlight();
            }
            return;
        }

        // 2. 사운드 팝업
        if (isSoundPopupActive)
        {
            Vector2 axis = stickInput.action.ReadValue<Vector2>();
            float now = Time.unscaledTime;
            int maxIdx = 3; // 0:Master, 1:BGM, 2:SFX, 3:닫기

            // 위/아래
            if (Mathf.Abs(axis.y) > 0.6f && now - soundPopupLastInputTime > soundPopupInputBufferTime)
            {
                int dir = (axis.y > 0) ? -1 : 1;
                soundPopupSelection += dir;
                soundPopupSelection = Mathf.Clamp(soundPopupSelection, 0, maxIdx);
                soundPopupLastInputTime = now;
                UpdateSoundPopupHighlight();
            }
            // 슬라이더 조정 (0~2만)
            if (soundPopupSelection <= 2 && Mathf.Abs(axis.x) > 0.2f)
            {
                Slider[] sliders = { masterSlider, bgmSlider, sfxSlider };
                float newValue = sliders[soundPopupSelection].value + axis.x * sliderSensitivity * Time.unscaledDeltaTime;
                sliders[soundPopupSelection].value = Mathf.Clamp01(newValue);
            }
            return;
        }

        // 3. 메인 메뉴
        Vector2 axisMenu = stickInput.action.ReadValue<Vector2>();
        float nowMenu = Time.unscaledTime;
        int menuMax = 2; // 0:Sound, 1:Continue, 2:Exit

        if (Mathf.Abs(axisMenu.y) > 0.6f && nowMenu - lastInputTime > inputBufferTime)
        {
            int dir = (axisMenu.y > 0) ? -1 : 1;
            int idx = (int)currentSelection + dir;
            idx = Mathf.Clamp(idx, 0, menuMax);
            currentSelection = (MenuItem)idx;
            lastInputTime = nowMenu;
            UpdateMenuHighlight();
        }
    }

    // ---------- 메뉴팝업 하이라이트 ----------
    void UpdateMenuHighlight()
    {
        // "음량조절" 하이라이트
        txtSound.color = (currentSelection == MenuItem.Sound) ? highlightColor : normalColor;
        txtContinue.color = (currentSelection == MenuItem.Continue) ? highlightColor : normalColor;
        txtExit.color = (currentSelection == MenuItem.Exit) ? highlightColor : normalColor;
    }

    // ---------- 사운드팝업 하이라이트 ----------
    void UpdateSoundPopupHighlight()
    {
        // 전체 normal

        masterLabel.color = normalColor;
        bgmLabel.color = normalColor;
        sfxLabel.color = normalColor;
        closeText.color = normalColor;

        masterSlider.handleRect.GetComponent<Image>().color = normalColor;
        bgmSlider.handleRect.GetComponent<Image>().color = normalColor;
        sfxSlider.handleRect.GetComponent<Image>().color = normalColor;

        // 현재 인덱스만 highlight
        switch ((SoundPopupItem)soundPopupSelection)
        {
            case SoundPopupItem.Master:
                masterLabel.color = sliderHighlightColor;
                masterSlider.handleRect.GetComponent<Image>().color = sliderHighlightColor;
                break;
            case SoundPopupItem.BGM:
                bgmLabel.color = sliderHighlightColor;
                bgmSlider.handleRect.GetComponent<Image>().color = sliderHighlightColor;
                break;
            case SoundPopupItem.SFX:
                sfxLabel.color = sliderHighlightColor;
                sfxSlider.handleRect.GetComponent<Image>().color = sliderHighlightColor;
                break;
            case SoundPopupItem.Close:
                closeText.color = highlightColor;
                break;
        }
    }

    // ---------- 게임종료팝업 하이라이트 ----------
    void UpdateExitPopupHighlight()
    {
        if (exitPopupContinueText != null)
            exitPopupContinueText.color = (exitPopupSelection == ExitPopupMenu.Continue) ? highlightColor : normalColor;
        if (exitPopupEndGameText != null)
            exitPopupEndGameText.color = (exitPopupSelection == ExitPopupMenu.Exit) ? highlightColor : normalColor;
    }

    // ---------- 트리거 처리 ----------
    void OnTriggerClick(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        // 사운드팝업
        if (isSoundPopupActive)
        {
            if (soundPopupSelection == 3) // "닫기"
            {
                CloseSoundPopup();
            }
            return;
        }
        // 게임종료팝업
        if (isExitPopupActive)
        {
            switch (exitPopupSelection)
            {
                case ExitPopupMenu.Continue: CloseExitPopup(); break;
                case ExitPopupMenu.Exit: ExitGame(); break;
            }
            return;
        }
        // 메뉴팝업
        switch (currentSelection)
        {
            case MenuItem.Sound: OpenSoundPopup(); break;
            case MenuItem.Continue: ClosePopup(); break;
            case MenuItem.Exit: OpenExitPopup(); break;
        }
    }

    void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        if (isPaused) ClosePopup();
        else OpenPopup();
    }

    // ---------- 팝업 열고닫기 ----------
    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        MoveToCameraFront();
        Time.timeScale = 0;
        isPaused = true;
        currentSelection = MenuItem.Sound;
        UpdateMenuHighlight();
    }
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
        if (isExitPopupActive)
        {
            exitPopupPanel.SetActive(false);
            isExitPopupActive = false;
        }
        if (isSoundPopupActive)
        {
            soundPopupPanel.SetActive(false);
            isSoundPopupActive = false;
        }
        currentSelection = MenuItem.Sound;
        UpdateMenuHighlight();
    }
    public void OpenExitPopup()
    {
        exitPopupPanel.SetActive(true);
        isExitPopupActive = true;
        exitPopupSelection = ExitPopupMenu.Continue;
        UpdateExitPopupHighlight();
    }
    public void CloseExitPopup()
    {
        exitPopupPanel.SetActive(false);
        isExitPopupActive = false;
    }
    public void ExitGame()
    {
        Debug.Log("게임 종료합니다");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- 사운드팝업 열고닫기 ----------
    public void OpenSoundPopup()
    {
        if (soundPopupPanel != null)
            soundPopupPanel.SetActive(true);
        isSoundPopupActive = true;
        soundPopupSelection = 0;
        UpdateSoundPopupHighlight();
    }
    public void CloseSoundPopup()
    {
        if (soundPopupPanel != null)
            soundPopupPanel.SetActive(false);
        isSoundPopupActive = false;
    }

    void MoveToCameraFront()
    {
        if (xrCamera == null)
            xrCamera = FindXRCamera();
        if (xrCamera == null) return;
        popupPanel.transform.position = xrCamera.position + xrCamera.forward * popupDistance;
        popupPanel.transform.rotation = Quaternion.LookRotation(xrCamera.forward, xrCamera.up);
    }
}
