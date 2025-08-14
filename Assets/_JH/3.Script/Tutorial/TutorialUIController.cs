using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIController : MonoBehaviour
{
    public Canvas rootCanvas;
    public Text titleText;
    public Text descText;
    public Text stepsText;
    public Image iconImage;

    [Header("배치 옵션")]
    public float distance = 0.9f;
    public float verticalOffset = -0.1f;
    public float followSlerp = 12f;

    private Transform followTarget;
    private bool visible;

    private void LateUpdate()
    {
        if (!visible || followTarget == null) return;

        // 위치/ 회전 (카메라 정면)
        Vector3 targetPos = followTarget.position + followTarget.forward * distance + Vector3.up * verticalOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSlerp);

        Quaternion lookRot = Quaternion.LookRotation(transform.position - followTarget.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * followSlerp);
    }

    public void ShowInFrontOfCamera(Transform cam, string title, string desc, string [] steps, Sprite icon)
    {
        followTarget = cam;
        titleText.text = title;
        descText.text = desc;

        if (steps != null && steps.Length > 0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var s in steps) if (!string.IsNullOrWhiteSpace(s)) sb.AppendLine("• " + s);
            stepsText.text = sb.ToString().TrimEnd();
        }
        else stepsText.text = "";

        iconImage.gameObject.SetActive(icon != null);

        rootCanvas.enabled = true;
        visible = true;
    }

    public IEnumerator HideAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        HideNow();
    }

    public void HideNow()
    {
        visible = false;
        rootCanvas.enabled = false;
        followTarget = null;
    }
}
