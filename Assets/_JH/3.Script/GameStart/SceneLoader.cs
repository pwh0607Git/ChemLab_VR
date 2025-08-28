using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("다음 씬")]
    [SerializeField] string nextSceneName = "Scn.InGame";
    [SerializeField] int nextSceneIndex = -1;

    [Header("로딩 보드(월드 공간용)")]
    [SerializeField] GameObject loadingRoot; // 월드 스페이스 캔버스/보드 (처음엔 비활성)
    [SerializeField] TMP_Text loadingText;  // "AI 준비 중…" 같은 문구
    [SerializeField] Transform spinner;      // 선택: 돌아가는 오브젝트(아이콘/원)
    [SerializeField] float spinnerSpeed = 270f; // 도/초
    [SerializeField] float minShowSeconds = 0.6f;

    bool isLoading;
    CancellationTokenSource cts;

    //  XR Simple Interactable의 Select Entered 이벤트에 이 메서드를 연결하세요.
    public async void StartGame()
    {
        if (isLoading) return;
        isLoading = true;

        ShowLoading("AI 초기화 중");

        cts?.Cancel();
        cts = new CancellationTokenSource();
        _ = AnimateLoading(cts.Token);

        // 1) Gemini 초기화/프리웜 대기 (타임아웃 허용)
        var mgr = GeminiAPIManager.Instance;
        if (mgr != null)
        {
            bool inited = await mgr.EnsureInitializedAsync(8000);
            if (!inited) SetStatus("AI 초기화 지연… 계속 진행");
            else
            {
                SetStatus("학습 응답 준비 중");
                bool warmed = await mgr.EnsurePrewarmAsync(8000);
                if (!warmed) SetStatus("네트워크 지연… 계속 진행");
            }
        }
        else
        {
            SetStatus("AI 매니저 없음… 계속 진행");
        }

        // 2) 로딩 보드 최소 노출 보장
        await Task.Delay((int)(minShowSeconds * 1000));

        // 3) 로딩 종료 후 씬 전환
        cts.Cancel();
        HideLoading();
        isLoading = false;

        if (nextSceneIndex >= 0) SceneManager.LoadScene(nextSceneIndex);
        else if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
        else Debug.LogError("[SceneLoader] 다음 씬이 설정되지 않았습니다.");
    }

    // ▶ Quit 버튼 이벤트
    public void Quit()
    {
        Debug.Log("[SceneLoader] Quit 호출됨");

#if UNITY_EDITOR
        // 에디터에서 플레이 중지
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 실행 파일 종료
        Application.Quit();
#endif
    }

    void ShowLoading(string msg)
    {
        if (loadingRoot) loadingRoot.SetActive(true);
        if (loadingText) loadingText.text = msg;
    }

    void SetStatus(string msg)
    {
        if (loadingText) loadingText.text = msg;
    }

    void HideLoading()
    {
        if (loadingRoot) loadingRoot.SetActive(false);
    }

    async Task AnimateLoading(CancellationToken token)
    {
        int dots = 0;
        while (!token.IsCancellationRequested)
        {
            // 점점점 효과
            if (loadingText)
            {
                string baseText = loadingText.text.Split('…')[0].TrimEnd('.', '·');
                loadingText.text = baseText + new string('·', dots + 1);
                dots = (dots + 1) % 3;
            }

            // 스피너 회전
            if (spinner) spinner.Rotate(0f, spinnerSpeed * Time.deltaTime, 0f, Space.Self);

            try { await Task.Delay(300, token); } catch { break; }
        }
    }
}
