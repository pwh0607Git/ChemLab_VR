// TutorialItem.cs  (SO 없애고 간단 버전)
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class TutorialItem : XRSimpleInteractable
{
    [Header("AIConversation의 Intent 키 (예: Tutorial_Torch)")]
    public string intentKey;

    [Header("UI 표기용(선택)")]
    public string displayName;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (string.IsNullOrEmpty(intentKey))
        {
            Debug.LogWarning($"[TutorialItem] intentKey가 비어있습니다: {name}");
            return;
        }
        AIGuideRouter.Instance?.RequestByIntent(intentKey, displayName);
    }
}
