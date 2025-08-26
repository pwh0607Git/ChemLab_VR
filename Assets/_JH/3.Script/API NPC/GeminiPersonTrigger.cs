using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

[RequireComponent(typeof(XRBaseInteractable))]
public class GeminiPersonTrigger : MonoBehaviour
{
    // APIConversation 안에 "Learning" intent가 있어야함
    [SerializeField] private string intentKey = "Learning";

    [Header("Hint UI")]
    [SerializeField] private GameObject questHint; // NPC 위에 띄울 힌트 텍스트
    [SerializeField] private CanvasGroup hintCanvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeOutSec = 0.4f;

    private XRBaseInteractable interactable;

    private void Reset()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnPlayerClick);

        if (questHint && !hintCanvasGroup)
            hintCanvasGroup = questHint.GetComponent<CanvasGroup>();

        if (questHint && !hintCanvasGroup)
            hintCanvasGroup = questHint.AddComponent<CanvasGroup>();

        if (questHint != null)
        {
            questHint.SetActive(true); // 시작할 때 힌트 켜기
            if (hintCanvasGroup) hintCanvasGroup.alpha = 1f;

        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnPlayerClick);
    }

    private void OnPlayerClick(SelectEnterEventArgs _)
    {
        // 1. Gemini 호출
        GeminiAPIManager.Instance.SendMessage(intentKey);

        // 2. 힌트 페이드아웃 후 비활성화
        if (questHint)
        {
            if(hintCanvasGroup)
            {
                hintCanvasGroup.DOKill();
                hintCanvasGroup.DOFade(0f, fadeOutSec)
                    .OnComplete(() => questHint.SetActive(false));
            }
            else
            {
                questHint.SetActive(false);
            }
        }
    }
}
