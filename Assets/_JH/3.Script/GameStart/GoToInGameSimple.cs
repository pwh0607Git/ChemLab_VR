// GoToInGameSimple.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class GoToInGameSimple : XRSimpleInteractable
{
    [Header("이동할 인게임 씬 이름")]
    public string nextSceneName = "Scn_InGame";

    [Header("있으면 사용 (StartGame에서 쓰던 로더)")]
    public SceneLoader sceneLoader; // 없어도 됨

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (sceneLoader != null) SceneManager.LoadScene(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }
}
