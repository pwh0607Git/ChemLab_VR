using UnityEngine;
using DG.Tweening;

public class QuestHintBillboard : MonoBehaviour
{
    [Header("Billboard")]
    public Transform targetCamera;
    public bool onlyYaw = true;

    [Header("Idle Float")]
    public float floatAmp = 0.05f;
    public float floatSec = 1.6f;

    Vector3 baseLocalPos;
    Tween floatTween;

    private void Awake()
    {
        if(!targetCamera)
        {
            var cam = Camera.main;
            if (cam) targetCamera = cam.transform;
        }
        baseLocalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        floatTween?.Kill();
        transform.localPosition = baseLocalPos;
    }

    private void LateUpdate()
    {
        if (!targetCamera) return;

        if(onlyYaw)
        {
            Vector3 lookPos = targetCamera.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }
        else
        {
            transform.LookAt(targetCamera);
        }
        //글자가 뒤집히지 않도록 카메라를 향하게
        transform.Rotate(0f, 180f, 0f);
    }
}
