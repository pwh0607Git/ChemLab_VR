using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TorchFlameController : MonoBehaviour
{
    [Header("VFX Manager 설정")]
    public VFXFlag torchFlameFlag = VFXFlag.FlameFx2;
    public Transform vfxAnchor;
    public bool loopFlame = true;

    [Header("XR Grab 동작")]
    [SerializeField] XRGrabInteractable grab;
    [SerializeField] bool turnOnWhenGrabbed = true; // ← 필요 없으면 false

    [Header("버튼으로 점화 제어 (Input System)")]
    [Tooltip("‘불 켜기’로 쓸 액션. 예: RightHand/primaryButton, triggerPressed 등")]
    [SerializeField] InputActionReference flameAction;  // ← 추가
    [Tooltip("버튼을 누르는 동안 ON, 떼면 OFF (true). false면 누를 때마다 토글.")]
    [SerializeField] bool holdToFlame = true;           // ← 추가
    [Tooltip("그랩 중일 때만 버튼 입력을 받도록 제한")]
    [SerializeField] bool requireGrabToUse = true;       // ← 추가

    [Header("쏘면서 태우기(점화 판정)")]
    [SerializeField] LayerMask igniteMask;
    [Tooltip("불 길이( Speed * Lifetime 에 맞추기")]
    [SerializeField, Min(0f)] float flameLength = 1.2f;
    [SerializeField, Min(0f)] float flameRadius = 0.035f;
    [SerializeField, Min(0f)] float igniteCooldown = 0.05f;
    [SerializeField] bool drawDebugRay = false;
    [Tooltip("노즐 앞에서 얼마나 떨어진 지점부터 판정을 시작할지")]
    [SerializeField, Min(0f)] float nozzleOffset = 0.2f;

    [Header("토치 사운드 클립")]
    public AudioClip torchFireClip;
    public AudioClip torchLoopClip;
    [SerializeField, Min(0f)] float loopFadeOut = 0.25f;
    private int _loopSfxId = 0; // 루프 사운드 인스턴스 ID

    public bool IsOn { get; private set; } = false;

    private VFX _flameVfx;
    float _igniteTimer;
    readonly Collider[] _hits = new Collider[8];

    // --- NEW: 내부 상태
    bool _isGrabbed = false;

    private void Reset()
    {
        if (!vfxAnchor)
        {
            var t = transform.Find("TorchVFXAnchor");
            if (t) vfxAnchor = t;
        }
        if (!grab) grab = GetComponent<XRGrabInteractable>();
    }

    private void Awake()
    {
        if (!vfxAnchor) vfxAnchor = transform;
        if (!grab) grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grab)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }

        // (선택) 시작부터 항상 액션을 켜두고 싶다면 Enable()만 호출하고 콜백은 그랩에서 연결
        if (flameAction) flameAction.action.Enable();
    }

    private void OnDisable()
    {
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
        UnsubscribeFlameAction();
        SetFlame(false); // 안전 정리
        if (flameAction) flameAction.action.Disable();
    }

    void Update()
    {
        // 불 VFX 위치/회전 동기화
        if (_flameVfx != null && vfxAnchor != null)
        {
            var t = _flameVfx.transform;
            t.position = vfxAnchor.position;
            t.rotation = vfxAnchor.rotation;
        }

        if (!IsOn || !vfxAnchor) return;

        _igniteTimer -= Time.deltaTime;
        if (_igniteTimer > 0f) return;

        // 불꽃 부피 판정
        Vector3 p0 = vfxAnchor.position + vfxAnchor.forward * nozzleOffset;
        Vector3 p1 = p0 + vfxAnchor.forward * flameLength;
        int count = Physics.OverlapCapsuleNonAlloc(
            p0, p1, flameRadius, _hits, igniteMask, QueryTriggerInteraction.Collide);

        if (drawDebugRay)
        {
            Debug.DrawLine(p0, p1, Color.yellow, 0f, false);
            Debug.DrawRay(p0, vfxAnchor.up * flameRadius, Color.red, 0f, false);
        }

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (!col) continue;

            var wick = col.GetComponentInParent<WickIgnitable>();
            if (wick != null)
            {
                wick.TryIgnite();
                _igniteTimer = igniteCooldown;
                break;
            }
        }
    }

    // XR Grab 이벤트
    void OnGrab(SelectEnterEventArgs _)
    {
        _isGrabbed = true;
        if (turnOnWhenGrabbed) SetFlame(true);
        SubscribeFlameAction();   // ← 버튼 입력 리스너 연결
    }

    void OnRelease(SelectExitEventArgs _)
    {
        _isGrabbed = false;
        if (turnOnWhenGrabbed) SetFlame(false);
        UnsubscribeFlameAction(); // ← 버튼 입력 리스너 해제
        if (holdToFlame) SetFlame(false);  // 홀드 방식이면 놓자마자 OFF 보장
    }

    // --- NEW: 액션 구독/해제
    void SubscribeFlameAction()
    {
        if (flameAction == null) return;
        var a = flameAction.action;
        a.performed -= OnFlamePerformed; // 중복 방지
        a.canceled -= OnFlameCanceled;
        a.performed += OnFlamePerformed;
        a.canceled += OnFlameCanceled;
    }

    void UnsubscribeFlameAction()
    {
        if (flameAction == null) return;
        var a = flameAction.action;
        a.performed -= OnFlamePerformed;
        a.canceled -= OnFlameCanceled;
    }

    // --- NEW: 액션 콜백
    void OnFlamePerformed(InputAction.CallbackContext ctx)
    {
        if (requireGrabToUse && !_isGrabbed) return;

        if (holdToFlame)
            SetFlame(true);     // 누르는 순간 ON
        else
            SetFlame(!IsOn);
    }

    void OnFlameCanceled(InputAction.CallbackContext ctx)
    {
        if (requireGrabToUse && !_isGrabbed) return;
        if (holdToFlame)
            SetFlame(false);     // 사운드는 SetFlame에서 처리
    }

    public void SetFlame(bool on)
    {
        if (on == IsOn) return;
        IsOn = on;

        if (on)
        {
            if (_flameVfx == null)
            {
                var anchor = vfxAnchor != null ? vfxAnchor : transform;
                _flameVfx = VFXManager.Instance.SpawnVFX(
                    torchFlameFlag, Vector3.zero, anchor.rotation, anchor, loopFlame);
                if (_flameVfx == null)
                {
                    Debug.LogWarning("[토치] 불 VFX가 스폰되지 않았습니다");
                    IsOn = false;
                    return;
                }
            }
            else
            {
                _flameVfx.Play();
            }

            // 여기서 오디오 시작
            StartTorchAudio();
        }
        else
        {
            if (_flameVfx != null)
            {
                _flameVfx.Stop();
                _flameVfx = null;
            }

            // 여기서 오디오 정지(루프 페이드아웃)
            StopTorchAudio();
        }
    }

    // SFX
    void StartTorchAudio()
    {
        var anchor = vfxAnchor != null ? vfxAnchor : transform;

        // 점화 원샷
        if (torchFireClip)
            SoundManager.Instance.PlaySFXOn(torchFireClip, anchor, loop: false, volume: 1f, pitch: 1f);

        // 히스/불꽃 루프 (중복 재생 방지)
        if (_loopSfxId == 0 && torchLoopClip)
            _loopSfxId = SoundManager.Instance.PlaySFXOn(torchLoopClip, anchor, loop: true, volume: 1f, pitch: 1f);
    }
    void StopTorchAudio()
    {
        // 루프 사운드만 부드럽게 페이드아웃
        if (_loopSfxId != 0)
        {
            SoundManager.Instance.StopSFXById(_loopSfxId, immediate: false, fadeOutSeconds: loopFadeOut);
            _loopSfxId = 0;
        }
        // (안전망) 혹시 남아있을 수 있는 동일 클립 일괄 정지 원하면 아래 주석 해제
        if (torchFireClip) SoundManager.Instance.StopSFX(torchFireClip, immediate: true);
        if (torchLoopClip) SoundManager.Instance.StopSFX(torchLoopClip, immediate: true);
    }
}
