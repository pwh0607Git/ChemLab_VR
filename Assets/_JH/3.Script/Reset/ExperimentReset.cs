using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class ExperimentReset : MonoBehaviour
{
    [Header("핵심 레퍼런스")]
    [SerializeField] TorchFlameController torch;
    [SerializeField] EruptionSequenceVFX eruption;

    [Header("리셋할 tranform들")]
    [SerializeField] Transform[] restoreTargets;

    [Header("입자/그래프 루트)")]
    [SerializeField] Transform[] extraEffectRoots;

    [Header("옵션")]
    [SerializeField] bool alsoResetRigidbodies = true;

    // vr입력
    [Header("VR 입력 설정")]
    [SerializeField] InputActionReference resetAction;
    [SerializeField] bool useLeftSecondaryPolling = true;
    [SerializeField] bool useRightSecondaryPolling = false;

    struct Snapshot
    {
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
        if(restoreTargets != null)
        {
            foreach(var t in restoreTargets)
            {
                if (!t) continue;
                _snapshot.Add(new Snapshot
                {
                    t = t,
                    pos = t.position,
                    rot = t.rotation,
                    scale = t.localScale,
                    active = t.gameObject.activeSelf
                }) ;
            }
        }
    }

    private void OnEnable()
    {
        if(resetAction && resetAction.action != null)
        {
            resetAction.action.performed += OnResetPerformed;
            resetAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (resetAction && resetAction.action != null)
        {
            resetAction.action.performed -= OnResetPerformed;
            resetAction.action.Disable();
        }
    }

    private void Update()
    {
        if (resetAction && resetAction.action != null)
            return;

        bool pressed = false;

        if (useLeftSecondaryPolling)
        {
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (left.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v)
                pressed = true;  // 왼손의 secondary(Y) 버튼
        }
        else if (useRightSecondaryPolling)
        {
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (right.TryGetFeatureValue(UnityEngine.InputSystem.CommonUsages.secondaryButton, out bool v) && v)
                pressed = true;  // 오른손의 secondary(B) 버튼
        }

        if (pressed && !_prevPressed)
            ResetAll();

        _prevPressed = pressed;
    }

    void OnResetPerformed(InputAction.CallbackContext ctx)
    {
        ResetAll();
    }

    [ContextMenu("Reset Now")]
    public void ResetAll()
    {
        // 1. 토치 불 off
        if (torch) torch.SetFlame(false);

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
        foreach(var s in _snapshot)
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
        if(alsoResetRigidbodies)
        {
            foreach(var rb in FindObjectsOfType<Rigidbody>(true))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 6. 강제 소거
        foreach (var wick in FindObjectsOfType<WickIgnitable>(true))
            wick.Extinguish();

        Debug.Log("[ExperimentReset] Reset completed.");
    }
}
