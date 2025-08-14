using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableTutorialHint : MonoBehaviour
{
    public TutorialItemData data;


    [Header("잡은 뒤에도 계속 표시)")]
    public bool stickyMode = true; // true면 놓아도 UI 유지

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnSelectEntered);
        grab.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {

        Debug.Log($"[Tutorial] SelectEntered: {name}, data={(data ? data.displayName : "null")}");
        if (data != null)
        {
            if(data) TutorialDirector.Instance?.ShowTutorial(data, persistWhileHeld : true);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {

        Debug.Log($"[Tutorial] SelectExited: {name}");
        if (!stickyMode) TutorialDirector.Instance?.HideTutorial();
    }
}

