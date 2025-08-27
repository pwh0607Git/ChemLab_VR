// TutorialTriggerOnSelect.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRBaseInteractable))] // Grab이든 Simple이든 상관없음
public class TutorialTriggerOnSelect : MonoBehaviour
{
    [Header("AIConversation의 Intent 키 (예: Tutorial_Liquid)")]
    public string intentKey;

    [Header("UI 표기용(선택)")]
    public string displayName;

    XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
        // 필요하면 hover만으로도 실행하고 싶을 때:
        // interactable.hoverEntered.AddListener(_ => Fire());
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs _)
    {
        Fire();
    }

    void Fire()
    {
        if (string.IsNullOrEmpty(intentKey))
        {
            Debug.LogWarning($"[TutorialTriggerOnSelect] intentKey 비어있음: {name}");
            return;
        }

        // 텍스트 패널에 '불러오는 중...' 먼저
        AIGuideRouter.Instance?.RequestByIntent(intentKey, displayName);

        // (선택) 진행상태 기록을 쓰고 있다면
        // TutorialProgress.MarkVisited(intentKey);
    }
}
