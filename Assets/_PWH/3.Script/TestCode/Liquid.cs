using UnityEngine;

public class Liquid : MonoBehaviour
{
    [SerializeField] private Transform head;

    [Header("Pour Angle")]
    public float angleThreshold = 150f;

    [Header("Flag")]
    private bool isGrab;
    [SerializeField] private bool isPour;
    private VFX pour;

    void Start()
    {
        isGrab = false;
        isPour = false;

        pour = VFXManager.Instance.SpawnVFX(VFXFlag.LiquidPour, Vector3.zero, Quaternion.Euler(Vector3.zero), head, true);
        pour.Stop(); // 시작할 때는 꺼둡니다.
    }

    void OnDisable()
    {
        if (pour == null) return;
        
        pour.Despawn();  
        pour = null;
    }

    void Update()
    {
        float angle = Vector3.Angle(head.up, Vector3.up);

        if (angle > angleThreshold)
        {
            isPour = true;
        }
        else
        {
            isPour = false;
        }
        
        if (isPour)
            pour.Play();
        else
            pour.Stop();
    }
}