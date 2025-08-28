using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPopup : MonoBehaviour
{
    public InputActionReference menuButtonAction; // XRI LeftHand > menuButton
    public Transform xrCamera; // XR Origin의 Main Camera
    public GameObject popupPanel; // 켜고 끌 패널 (Canvas의 자식으로 연결)

    public float popupDistance = 2.0f;

    private void OnEnable()
    {
        Debug.Log("MenuPopup OnEnable!");
        var actionMap = menuButtonAction.action.actionMap;
        if (actionMap != null)
        {
            Debug.Log("ActionMap: " + actionMap.name + ", Enabled: " + actionMap.enabled);
            if (!actionMap.enabled) actionMap.Enable();
            Debug.Log("ActionMap Now Enabled: " + actionMap.enabled);
        }

        menuButtonAction.action.performed += OnMenuButtonPressed;
        menuButtonAction.action.Enable();
        Debug.Log("Menu button action enabled? " + menuButtonAction.action.enabled);
    }

    private void OnDisable()
    {
        menuButtonAction.action.performed -= OnMenuButtonPressed;
        menuButtonAction.action.Disable();
    }

    void OnMenuButtonPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("메뉴버튼 눌림!");
        // 패널만 토글
        if (popupPanel == null) return;

        bool isActive = popupPanel.activeSelf;
        popupPanel.SetActive(!isActive);

        // 열릴 때만 카메라 앞에 위치시키기
        if (!isActive)
            MoveToCameraFront();
    }

    void MoveToCameraFront()
    {
        if (xrCamera == null) xrCamera = Camera.main.transform;
        // 캔버스(자기 자신)를 카메라 앞에 이동시킨다
        transform.position = xrCamera.position + xrCamera.forward * popupDistance;
        transform.rotation = Quaternion.LookRotation(xrCamera.forward, Vector3.up);
    }

    public void clicktest()
    {

        Debug.Log("클릭테스트");
    }
    public void clicktest2()
    {

        Debug.Log("클릭테스트2");
    }
}
