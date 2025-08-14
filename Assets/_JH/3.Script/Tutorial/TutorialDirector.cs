using UnityEngine;

public interface ITtsSpeaker
{
    bool Speak(string text);
}

public class TutorialDirector : MonoBehaviour
{
    public static TutorialDirector Instance { get; private set; }

    [Header("참조")]
    public Camera playerCamera;
    public TutorialUIController uIController;
    public AudioSource voiceSource;
    public MonoBehaviour ttsSpeakerBehaviour;
    private ITtsSpeaker tts;

    private Coroutine hideCo;

    private void Awake()
    {
        if( Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        tts = ttsSpeakerBehaviour as ITtsSpeaker;
    }

    public void ShowTutorial(TutorialItemData data, bool persistWhileHeld)
    {
        if (uIController == null || playerCamera == null || data == null) return;

        // 카메라 앞에 UI 배치/업데이트
        uIController.ShowInFrontOfCamera(playerCamera.transform, data.displayName, data.shortDescription, data.stepHints, data.icon);

        // 오디오 : TTS 우선, 실패 시 Fallback Clip
        bool ttsStarted = false;
        if (tts != null && !string.IsNullOrWhiteSpace(data.ttsKorean))
            ttsStarted = tts.Speak(data.ttsKorean);

        if (!ttsStarted && data.voiceClipFallback != null)
        {
            voiceSource.Stop();
            voiceSource.clip = data.voiceClipFallback;
            voiceSource.Play();
        }

        // 자동 숨김 타이머 ( 잡고 있는 동안 유지 옵션)
        if (hideCo != null) StopCoroutine(hideCo);
        if (!persistWhileHeld)
            hideCo = StartCoroutine(uIController.HideAfter(data.uiShowDuration));
    }

    public void HideTutorial()
    {
        if (uIController == null) return;
        if (hideCo != null) StopCoroutine(hideCo);
        uIController.HideNow();
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();
    }
}
