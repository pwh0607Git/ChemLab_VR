using UnityEngine;

public interface ITtsSpeaker { bool Speak(string text); }

// TutorialDirector.cs (핵심 부분만)
public class TutorialDirector : MonoBehaviour
{
    public static TutorialDirector Instance { get; private set; }

    public TutorialUIControllerStaticTMP uiController;  // 고정 캔버스
    public AudioSource voiceSource;
    public MonoBehaviour ttsSpeakerBehaviour;  // 선택
    private ITtsSpeaker tts;
    private Coroutine hideCo;

    void Awake() { if (Instance && Instance != this) { Destroy(gameObject); return; } Instance = this; }
    void Start() {tts = ttsSpeakerBehaviour as ITtsSpeaker; }

    public void ShowTutorial(TutorialItemData data, bool persistWhileHeld)
    {
        if (!data || !uiController) { Debug.LogWarning("[Director] missing ui/data"); return; }

        // 1) UI 내용 업데이트
        uiController.Show(data.displayName, data.shortDescription, data.stepHints, data.icon);


        if (hideCo != null) { StopCoroutine(hideCo); hideCo = null; }

        // if (!persistWhileHeld) hideCo = StartCoroutine(uiController.HideAfter(data.uiShowDuration));

        bool spoke = (tts != null && !string.IsNullOrWhiteSpace(data.ttsKorean)) && tts.Speak(data.ttsKorean);
        if (!spoke && voiceSource && data.voiceClipFallback) voiceSource.PlayOneShot(data.voiceClipFallback);
    }

    public void HideTutorial()
    {
        if (hideCo != null) StopCoroutine(hideCo);
        uiController?.HideNow();
        if (voiceSource && voiceSource.isPlaying) voiceSource.Stop();
    }
}

