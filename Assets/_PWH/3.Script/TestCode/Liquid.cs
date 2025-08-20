using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class PourBehaviour
{
    public Transform head;            // 기울기 측정 지점

    private VFX pourVFX;

    public void Initialize(Transform headTransform)
    {
        head = headTransform;

        pourVFX = VFXManager.Instance.SpawnVFX(
            VFXFlag.LiquidPour,
            Vector3.zero,
            Quaternion.identity,
            head,
            true
        );
        pourVFX.Stop();
    }

    public void Start()
    {
        pourVFX.Play();
    }

    public void Stop()
    {
        pourVFX.Stop();
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

    [SerializeField] private PourBehaviour pour;

    void Start()
    {
        pour = new PourBehaviour();
        pour.Initialize(head);
    }

    void Update()
    {
        float angle = Vector3.Angle(head.up, Vector3.up);

        UpdatePourState(angle > angleThreshold);
    }

    void UpdatePourState(bool on)
    {
        if (on) pour.Start();
        else pour.Stop();
    }

    void OnDisable()
    {
        pour.Dispose();
    }
}