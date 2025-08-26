using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Prompt{
    public string intent;           // 의도
    public string prompt;           // AI 출력 의도   
}

[CreateAssetMenu(fileName = "AIPrompt", menuName = "Gemini/Prompt")]
public class APIConversation : ScriptableObject
{
    public List<Prompt> prompts;
}