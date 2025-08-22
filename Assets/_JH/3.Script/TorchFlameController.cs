using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TorchFlameController : MonoBehaviour
{
    [Header("VFX Manager 설정")]
    public VFXFlag torchFlameFlag = VFXFlag.FlameFx2;  // 매니저에 등록된 토치 불  VFX 플래그
    public Transform vfxAnchor; // 불 위치( 없으면 이 오브젝트)
    public bool loopFlame = true; // 토치 불은 루프

    [Header("XR Grab 동작")]
    [SerializeField] XRGrabInteractable grab;
    [SerializeField] bool turnOnWhenGrabbed = true;

    [Header("쏘면서 태우기(점화 판정)")]
    [SerializeField] LayerMask igniteMask;
    [Tooltip("불 길이( Speed * Lifetime 에 맞추기")]
    [SerializeField, Min(0f)] float flameLength = 1.2f;
    [SerializeField, Min(0f)] float flameRadius = 0.035f;
    [SerializeField, Min(0f)] float igniteCooldown = 0.05f;
    [SerializeField] bool drawDebugRay = false;

    public bool IsOn { get; private set; } = false;

    // 스폰된 불 VFX 인스턴스
    private VFX _flameVfx;
    float _igniteTimer;

    readonly Collider[] _hits = new Collider[8];

    private void Reset()
    {
        if(!vfxAnchor)
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
    }
    private void OnDisable()
    {
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
        SetFlame(false); // 안전 정리
    }

    void Update()
    {
        // 불 켜져 있는 동안, 매 프레임 앵커와 동기화
        if (_flameVfx != null && vfxAnchor != null)
        {
            var t = _flameVfx.transform;
            t.position = vfxAnchor.position;
            t.rotation = vfxAnchor.rotation;   // 새로 나오는 입자는 항상 forward(+Z)로
        }

        if (!IsOn || !vfxAnchor) return;

        _igniteTimer -= Time.deltaTime;
        if (_igniteTimer > 0f) return;

        // 🔥 불꽃 "부피" 전체에 대한 겹침 판정 (노즐에서 앞으로 flameLength 길이)
        Vector3 p0 = vfxAnchor.position;                          // 시작점(노즐)
        Vector3 p1 = p0 + vfxAnchor.forward * flameLength;        // 끝점(제트 끝)
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

            if (col.TryGetComponent(out WickIgnitable wick))
            {
                wick.TryIgnite();            // 심지 점화
                _igniteTimer = igniteCooldown;
                break;                       // 한 번만 점화하고 다음 프레임에 다시 판정
            }
        }
    }

    // XR Grab 이벤트
    void OnGrab(SelectEnterEventArgs _)
    {
        if (turnOnWhenGrabbed) SetFlame(true);
    }

    void OnRelease(SelectExitEventArgs _)
    {
        if (turnOnWhenGrabbed) SetFlame(false); ;
    }

    public void SetFlame(bool on)
    {
        if (on == IsOn) return;
        IsOn = on;

        if (on)
        {
            // 켜기 :스폰 (이미 있으면 재생)
            if (_flameVfx == null)
            {
                var anchor = vfxAnchor != null ? vfxAnchor : transform;
                _flameVfx = VFXManager.Instance.SpawnVFX(
                    torchFlameFlag,
                    Vector3.zero,
                    anchor.rotation,
                    anchor, // 토치에 붙여서 이동
                    loopFlame
                    );
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
        }
        else
        {
            if(_flameVfx != null)
            {
                _flameVfx.Stop();
                _flameVfx = null;
            }
        }
    }
}
