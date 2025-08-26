using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("다음 씬 이름 ")]
    [SerializeField] private string nextSceneName = "Scn.InGame";
    [SerializeField] private int nextSceneIndex = -1;

    public void StartGame()
    {
        if (nextSceneIndex >= 0)
            SceneManager.LoadScene(nextSceneIndex);
        else if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogError("[SceneLoader] 다음씬이 없습니다.");
        }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터에선 바로 종료가 안 되므로 Play 중지
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
