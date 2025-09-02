// GoToInGameSimple.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class GoToInGameSimple : XRSimpleInteractable
{
    [Header("이동할 인게임 씬 이름")]
    public string nextSceneName = "Scn_InGame";

    [Header("있으면 사용 (StartGame에서 쓰던 로더)")]
    public SceneLoader sceneLoader; // 없어도 됨

    [Header("버튼 SFX")]
    public AudioClip clickClip;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        SoundManager.Instance.PlaySFX(clickClip, spatialBlendOverride: 0f);
        base.OnSelectEntered(args);
        if (sceneLoader != null) SceneManager.LoadScene(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }
}
