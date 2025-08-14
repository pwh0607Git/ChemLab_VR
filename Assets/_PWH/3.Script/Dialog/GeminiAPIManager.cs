using System.Collections;
using System.Collections.Generic;
using CustomInspector;
using Firebase;
using Firebase.AI;
using Firebase.Extensions;
using UnityEngine;

public class GeminiAPIManager : BehaviourSingleton<GeminiAPIManager>
{
    protected override bool IsDontDestroy() => false;

    [SerializeField, ReadOnly] private GenerativeModel model;
    [SerializeField, ReadOnly] private Chat chatSession;

    [Header("Input Part")]
    [SerializeField] string inputMessage;

    [Button("SendMessage"), HideField] public bool sendBtn1;
    public void SendMessage()
    {
        SendMessage(inputMessage);
    }

    void Start()
    {
        InitFB();
    }

    void InitFB()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Gemini 모델 인스턴스 생성
                // gemini-pro 또는 다른 모델명을 지정할 수 있습니다.
                model = FirebaseAI.GetInstance().GetGenerativeModel(modelName: "gemini-2.5-flash");

                // 채팅 세션 시작
                chatSession = model.StartChat();
                Debug.Log("Firebase와 Gemini가 성공적으로 초기화되었습니다.");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public async void SendMessage(string userMessage)
    {
        if (chatSession == null)
        {
            Debug.LogError("채팅 세션이 초기화되지 않았습니다.");
            return;
        }

        // 사용자의 메시지를 보내고 응답을 받습니다.
        var response = await chatSession.SendMessageAsync(userMessage);

        // 응답 텍스트를 출력합니다.
        Debug.Log($"Gemini 응답: {response.Text}");
    }
}