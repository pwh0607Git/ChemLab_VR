using UnityEngine;

public class SoftResetTransform : MonoBehaviour, ISoftResettable
{
    [Header("초기 포즈를 월드 기준으로 저장")]
    public bool useWorldSpace = true;

    [Header("리셋 시 Rigidbody 속도도 0")]
    public bool resetRigidbodyVelocity = true;

    [Header("리셋 시 원래 부모(Parent)로 되돌리기")]
    public bool restoreOriginalParent = true;

    [Header("시작 시 자동 캡처")]
    public bool captureOnStart = true;

    Vector3 initPos, initScale;
    Quaternion initRot;
    Transform initParent;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (captureOnStart) CaptureNow();
    }

    /// 현재 상태를 초기값으로 재설정
    public void CaptureNow()
    {
        initParent = transform.parent;

        if (useWorldSpace)
        {
            initPos = transform.position;
            initRot = transform.rotation;
        }
        else
        {
            initPos = transform.localPosition;
            initRot = transform.localRotation;
        }
        initScale = transform.localScale;
    }

    public void SoftReset()
    {
        // 부모 되돌리기 (XR Grab 등으로 부모가 변경된 경우)
        if (restoreOriginalParent && transform.parent != initParent)
            transform.SetParent(initParent, worldPositionStays: true);

        if (useWorldSpace)
        {
            transform.SetPositionAndRotation(initPos, initRot);
        }
        else
        {
            transform.localPosition = initPos;
            transform.localRotation = initRot;
        }
        transform.localScale = initScale;

        if (resetRigidbodyVelocity && rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
