using UnityEngine;

public class DummyTtsSpeaker : MonoBehaviour, ITtsSpeaker
{
    // 실제 TTS 연동 전까지는 false 반환
    public bool Speak(string text)
    {
        return false;
    }
}
