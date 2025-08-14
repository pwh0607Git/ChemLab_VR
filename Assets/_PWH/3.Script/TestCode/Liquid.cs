using System;
using UnityEngine;

[Serializable]
public class PourBehaviour
{
    public Transform head;            // 기울기 측정 지점
    public float angleThreshold = 150f;

    private bool isPouring;
    private VFX pourVFX;

    public bool IsPouring => isPouring;

    public void Initialize(Transform headTransform, float angleThreshold)
    {
        head = headTransform;
        isPouring = false;
        this.angleThreshold = angleThreshold;

        pourVFX = VFXManager.Instance.SpawnVFX(
            VFXFlag.LiquidPour,
            Vector3.zero,
            Quaternion.identity,
            head,
            true
        );
        pourVFX.Stop();
    }

    public void UpdatePour()
    {
        if (head == null) return;

        float angle = Vector3.Angle(head.up, Vector3.up);
        isPouring = angle > angleThreshold;

        if (isPouring)
            pourVFX?.Play();
        else
            pourVFX?.Stop();
    }

    public void Dispose()
    {
        if (pourVFX != null)
        {
            pourVFX.Despawn();
            pourVFX = null;
        }
    }
}

public class Liquid : MonoBehaviour
{
    [SerializeField] ChemFlag flag;

    public ChemFlag Flag => flag;

    [SerializeField] private Transform head;

    [Header("Pour Angle")]
    public float angleThreshold = 150f;

    [Header("Flag")]
    private bool isGrab;
    [SerializeField] private bool isPour;

    // 🔹 기능 클래스 인스턴스
    [SerializeField] private PourBehaviour pour;

    void Start()
    {
        pour = new PourBehaviour();
        pour.Initialize(head, angleThreshold);
    }

    void Update()
    {
        pour.UpdatePour();
    }

    void OnDisable()
    {
        pour.Dispose();
    }
}