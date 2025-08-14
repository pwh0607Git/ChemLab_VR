// TutorialUIControllerStaticTMP.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public class TutorialUIControllerStaticTMP : MonoBehaviour
{
    public Canvas rootCanvas;                    // World Space
    public TMP_Text titleText, descText, stepsText;
    public Image iconImage;

    private CanvasGroup cg;

    void Awake()
    {
        if (!rootCanvas) rootCanvas = GetComponent<Canvas>();
        cg = GetComponent<CanvasGroup>();
    }

    public void Show(string title, string desc, string[] steps, Sprite icon)
    {
        if (!rootCanvas) return;

        if (titleText) titleText.text = string.IsNullOrWhiteSpace(title) ? " " : title;
        if (descText) descText.text = string.IsNullOrWhiteSpace(desc) ? " " : desc;

        if (stepsText)
        {
            var sb = new StringBuilder();
            if (steps != null) foreach (var s in steps) if (!string.IsNullOrWhiteSpace(s)) sb.AppendLine("• " + s);
            stepsText.text = sb.Length > 0 ? sb.ToString().TrimEnd() : " ";
        }

        if (iconImage)
        {
            iconImage.gameObject.SetActive(icon != null);
            if (icon) iconImage.sprite = icon;
        }

        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false; }
        rootCanvas.enabled = true;               // 위치/회전은 건드리지 않음(씬에 둔 그대로)
    }

    public IEnumerator HideAfter(float sec) { yield return new WaitForSeconds(sec); HideNow(); }
    public void HideNow()
    {
        if (cg) cg.alpha = 0f;
        if (rootCanvas) rootCanvas.enabled = false;
    }

    [ContextMenu("TEST: Show sample")]
    void _TestShow() => Show("샘플 제목", "샘플 설명", new[] { "스텝 1", "스텝 2" }, null);
}
