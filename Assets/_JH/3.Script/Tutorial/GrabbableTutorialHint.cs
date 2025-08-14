using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableTutorialHint : MonoBehaviour
{
    public TutorialItemData data;

    private XRGrabInteractable grab;
    private bool isSelected;

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
        isSelected = true;
        if (data != null)
        {
            TutorialDirector.Instance?.ShowTutorial(data, persistWhileHeld : true);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isSelected = false;
        TutorialDirector.Instance?.HideTutorial();
    }
}

