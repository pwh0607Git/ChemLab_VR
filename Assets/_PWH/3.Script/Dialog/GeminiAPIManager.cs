using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomInspector;
using Firebase;
using Firebase.AI;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeminiAPIManager : BehaviourSingleton<GeminiAPIManager>
{
    protected override bool IsDontDestroy() => true;

    // ===== Firebase / Gemini =====
    [SerializeField, ReadOnly] private GenerativeModel model;
    [SerializeField, ReadOnly] private Chat chatSession;

    // ===== Prewarm =====
    [Header("Prewarm")]
    [SerializeField] private bool prewarmOnStart = true;
    [SerializeField] private string prewarmIntent = "Learning"; // 에셋의 Intent 키
    [SerializeField] private bool showPrewarmOutput = false;      // 시작씬에서 보통 꺼둠
    [SerializeField] private bool cachePrewarmOutput = true;       // 결과 캐시

    private bool prewarmRunning;

    private List<string> cachedLearning;
    private TaskCompletionSource<bool> prewarmTcs = new TaskCompletionSource<bool>();

    public bool IsReady => chatSession != null;
    public bool IsPrewarmReady => cachedLearning != null && cachedLearning.Count > 0;
    public bool IsPrewarmRunning => prewarmRunning;

    // ===== Input / Prompts / UI =====
    [Header("Input Part")]
    [SerializeField] private string inputMessage;

    [Header("Prompt DB")]
    [SerializeField] private APIConversation conversation;

    [Header("Textbox")]
    [SerializeField] private TextBoxController textbox;

    [Button(nameof(SendMessage)), HideField] public bool __btn;
    public void SendMessage() => SendMessage(inputMessage);

    void Start() => InitFB();
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode m) => TryBindTextBox();

    void TryBindTextBox()
    {
        if (textbox == null)
            textbox = FindObjectOfType<TextBoxController>(true);
    }

    void InitFB()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
                return;
            }

            model = FirebaseAI.GetInstance().GetGenerativeModel("gemini-2.5-flash");
            chatSession = model.StartChat();

            if (prewarmOnStart && !prewarmRunning)
                _ = PrewarmAsync(); // fire-and-forget

            Debug.Log("Firebase와 Gemini가 성공적으로 초기화되었습니다.");
        });
    }

    async Task PrewarmAsync()
    {
        prewarmTcs = new TaskCompletionSource<bool>();
        try
        {
            prewarmRunning = true;

            var item = FindPrompt(prewarmIntent);
            if (item == null)
            {
                Debug.LogWarning($"[Gemini] 프리웜 실패: intent '{prewarmIntent}'을 찾지 못했습니다.");
                prewarmTcs.TrySetResult(false);
                return;
            }

            var res = await chatSession.SendMessageAsync(item.prompt);
            var text = res.Text ?? string.Empty;

            if (cachePrewarmOutput && !string.IsNullOrEmpty(text))
                cachedLearning = SplitString(text);

            if (showPrewarmOutput && textbox != null && cachedLearning != null)
                ShowText(cachedLearning);

            prewarmTcs.TrySetResult(IsPrewarmReady);
            Debug.Log($"[Gemini] Prewarm done. cached={IsPrewarmReady}, len={text.Length}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Gemini] Prewarm error: " + ex.Message);
            prewarmTcs.TrySetResult(false);
        }
        finally { prewarmRunning = false; }
    }

    /// 인게임 들어가기 전에 프리웜 완료를 보장하고 싶다면 이걸 기다리면 됨.
    public async Task<bool> EnsurePrewarmAsync(int timeoutMs = 8000)
    {
        // 1) 모델/세션 초기화 먼저 보장(타임아웃)
        if (!await EnsureInitializedAsync(timeoutMs)) return false;

        // 2) 이미 준비되었으면 바로 true
        if (IsPrewarmReady) return true;

        // 3) 진행 중이 아니면 시작
        if (!prewarmRunning)
        {
            prewarmTcs = new TaskCompletionSource<bool>();
            _ = PrewarmAsync(); // fire and forget
        }

        // 4) 완료 또는 타임아웃까지 대기
        var finished = await Task.WhenAny(prewarmTcs.Task, Task.Delay(timeoutMs)) == prewarmTcs.Task;
        return finished && prewarmTcs.Task.Result && IsPrewarmReady;
    }


    public async Task<bool> EnsureInitializedAsync(int timeoutMs = 8000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (chatSession == null && sw.ElapsedMilliseconds < timeoutMs)
            await Task.Yield();
        return chatSession != null;
    }

    Prompt FindPrompt(string intent)
    {
        if (conversation == null || conversation.prompts == null) return null;
        return conversation.prompts.Find(p =>
            string.Equals(p.intent, intent, StringComparison.OrdinalIgnoreCase));
    }

    /// NPC 클릭 시 캐시가 있으면 즉시 표시(네트워크 대기 0초)
    public bool TryShowCachedLearning()
    {
        if (cachedLearning == null || cachedLearning.Count == 0) return false;
        ShowText(cachedLearning);
        // cachedLearning = null; // 한 번 쓰고 지우고 싶으면 주석 해제
        return true;
    }

    public async void SendMessage(string intent)
    {
        if (chatSession == null)
        {
            Debug.LogError("[Gemini] 채팅 세션이 초기화되지 않았습니다.");
            return;
        }

        var item = FindPrompt(intent);
        if (item == null || string.IsNullOrEmpty(item.prompt))
        {
            Debug.LogWarning($"[Gemini] intent '{intent}' 프롬프트를 찾지 못했습니다.");
            return;
        }

        var res = await chatSession.SendMessageAsync(item.prompt);
        var text = res.Text ?? string.Empty;
        Debug.Log($"Gemini 응답: {text}");

        ShowText(SplitString(text));
    }

    void ShowText(List<string> msgs)
    {
        TryBindTextBox();
        if (textbox == null || msgs == null || msgs.Count == 0) return;

        if (!textbox.gameObject.activeSelf)
            textbox.gameObject.SetActive(true);

        // TextBoxController에 맞춰서 큐 전달(네 프로젝트 API)
        textbox.SetTextQueue(msgs);
    }

    List<string> SplitString(string message)
    {
        if (string.IsNullOrEmpty(message)) return new List<string>();
        return message
            .Replace("\r\n", "\n")
            .Split(new[] { '.', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }
}
