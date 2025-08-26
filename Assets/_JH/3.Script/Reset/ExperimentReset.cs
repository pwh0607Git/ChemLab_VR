using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class ExperimentResetHard : MonoBehaviour
{
    [Header("트리거 입력 (둘 중 하나)")]
    [SerializeField] InputActionReference resetAction;    // 비워두면 폴링 사용
    [SerializeField] bool useLeftSecondaryPolling = true; // 왼손 Y
    [SerializeField] bool useRightSecondaryPolling = false; // 오른손 B

    [Header("어떤 씬을 다시 로드할지")]
    [SerializeField] bool reloadFirstLoaded = false;      // true=앱 처음 로드된 씬
    [SerializeField] string sceneName = "";               // 지정 이름(옵션, 비우면 무시)

    static string s_firstScene;
    bool _prev;

    void Awake()
    {
<<<<<<< Updated upstream
        if (string.IsNullOrEmpty(s_firstScene))
            s_firstScene = SceneManager.GetActiveScene().name;
    }

    void OnEnable()
=======
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public bool active;
    }
    readonly List<Snapshot> _snapshot = new List<Snapshot>();

    bool _prevPressed;

    private void Awake()
    {
        if (!torch) torch = FindObjectOfType<TorchFlameController>(true);
        if (!eruption) eruption = FindObjectOfType<EruptionSequenceVFX>(true);

        _snapshot.Clear();
        if (restoreTargets != null)
        {
            foreach (var t in restoreTargets)
            {
                if (!t) continue;
                _snapshot.Add(new Snapshot
                {
                    t = t,
                    pos = t.position,
                    rot = t.rotation,
                    scale = t.localScale,
                    active = t.gameObject.activeSelf
                });
            }
        }
    }

    private void OnEnable()
    {
        if (resetAction && resetAction.action != null)
        {
            resetAction.action.performed += OnResetPerformed;
            resetAction.action.Enable();
        }
    }

    private void OnDisable()
>>>>>>> Stashed changes
    {
        if (resetAction && resetAction.action != null)
        {
            resetAction.action.performed += OnReset;
            resetAction.action.Enable();
        }
    }
    void OnDisable()
    {
        if (resetAction && resetAction.action != null)
        {
            resetAction.action.performed -= OnReset;
            resetAction.action.Disable();
        }
    }

    void Update()
    {
        // 인풋 액션을 쓰면 폴링 생략
        if (resetAction && resetAction.action != null) return;

        bool pressed = false;
        if (useLeftSecondaryPolling)
        {
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (left.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v) pressed = true;
        }
        else if (useRightSecondaryPolling)
        {
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (right.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v) pressed = true;
        }

        if (pressed && !_prev) HardReset();
        _prev = pressed;
    }

    void OnReset(InputAction.CallbackContext _)
    {
        HardReset();
    }

    public void HardReset()
    {
        Time.timeScale = 1f; // 혹시 멈춰있으면 원복

<<<<<<< Updated upstream
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (reloadFirstLoaded)
            SceneManager.LoadScene(s_firstScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
=======
        // 2. 분출/연기/재 stop 및 내부 정리
        if (eruption) eruption.StopAllCoroutines();

        // 3. 모든 이펙트 안전 정지
        foreach (var v in FindObjectsOfType<VFX>(true))
            v.Stop();

        foreach (var ve in FindObjectsOfType<VisualEffect>(true))
            ve.Stop();

        foreach (var ps in FindObjectsOfType<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
        }

        // 4. 등록 타깃들 + 활성 상태 복원
        foreach (var s in _snapshot)
        {
            if (!s.t) continue;
            var go = s.t.gameObject;
            go.SetActive(true);
            s.t.position = s.pos;
            s.t.rotation = s.rot;
            s.t.localScale = s.scale;
            go.SetActive(s.active);
        }

        // 5. 물리 초기화
        if (alsoResetRigidbodies)
        {
            foreach (var rb in FindObjectsOfType<Rigidbody>(true))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 6. 강제 소거
        foreach (var wick in FindObjectsOfType<WickIgnitable>(true))
            wick.Extinguish();

        Debug.Log("[ExperimentReset] Reset completed.");
>>>>>>> Stashed changes
    }
}
