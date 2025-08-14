using UnityEngine;

[CreateAssetMenu(fileName = "TutorialItem", menuName ="Tutorial/Tutorial Item Data")]
public class TutorialItemData : MonoBehaviour
{
    [Header("표시")]
    public string displayName;
    [TextArea(2, 5)] public string shortDescription;
    [TextArea(3, 8)] public string[] stepHints;
    public Sprite icon;

    [Header("오디오/TTS")]
    [TextArea(3, 8)] public string ttsKorean; // TTS용 텍스트
    public AudioClip voiceClipFallback; // TTS가 없을 때 사용할 클립(선택)

    [Header("옵션")]
    public float uiShowDuration = 6f; // 기본 표시 시간
}
