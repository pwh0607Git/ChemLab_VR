// AIGuideRouter.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIGuideRouter : MonoBehaviour
{
    public static AIGuideRouter Instance { get; private set; }

    [Header("대화 데이터(네가 만든 AIConversation SO)")]
    public APIConversation conversation;

    [Header("출력용 Text 박스(네가 씬에 둔 TextBoxController)")]
    public TextBoxController textBox;

    [Header("표시 문자열")]
    public string waitingText = "불러오는 중...";

    void Awake() => Instance = this;

    /// 튜토리얼 요청(오브젝트 클릭 시 호출)
    public void RequestByIntent(string intentKey, string titleForUI = null)
    {
        if (!textBox) { Debug.LogWarning("[AIGuideRouter] textBox 미지정"); return; }

        // 1) 대기 문구 먼저 출력(큐에 1줄)
        if (!textBox.gameObject.activeSelf) textBox.gameObject.SetActive(true);
        textBox.SetTextQueue(new List<string> {
            string.IsNullOrEmpty(titleForUI) ? waitingText : $"{titleForUI}\n{waitingText}"
        });

        // 2) Gemini에 intent 전송 (프로젝트에서 이미 쓰던 방식 그대로)
        if (GeminiAPIManager.Instance != null)
        {
            GeminiAPIManager.Instance.SendMessage(intentKey);
            // 응답을 받는 곳에서 ↓ 이 한 줄만 호출해주면 TextBox에 뜹니다.
            // AIGuideRouter.Instance.OnGeminiResponse(responseText);
        }
        else
        {
            // 매니저가 없으면 임시로 intent만 표시(디버그)
            textBox.SetTextQueue(new List<string> { $"[{intentKey}] 응답 대기 실패" });
        }
    }

    /// Gemini 매니저가 응답을 받았을 때 호출해 주세요.
    public void OnGeminiResponse(string responseText)
    {
        if (!textBox) return;
        if (!textBox.gameObject.activeSelf) textBox.gameObject.SetActive(true);

        // TextBoxController는 큐 방식이므로 한 줄만 넣어도 자동 타이핑 후
        // 클릭 시 다음 줄로 진행합니다.
        textBox.SetTextQueue(new List<string> { string.IsNullOrWhiteSpace(responseText) ? "(빈 응답)" : responseText });
    }
}
