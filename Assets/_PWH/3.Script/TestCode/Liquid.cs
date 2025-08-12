using UnityEngine;

public class Liquid : MonoBehaviour
{
    [SerializeField] private Transform head;

    [Header("Flag")]
    private bool isGrab;
    [SerializeField] private bool isPour;
    private VFX pour;

    void Start()
    {
        isGrab = false;
        isPour = false;
    }

    void OnDisable()
    {
        if (pour == null) return;
        
        pour.Despawn();  
        pour = null;
    }

    void Update()
    {
        if (isPour)
        {
            if (pour == null)
            {
                pour = VFXManager.Instance.SpawnVFX(VFXFlag.LiquidPour, Vector3.zero, Quaternion.Euler(Vector3.zero), head, true);  
                pour.Play();
            }
        }
        else
        {
            if (pour != null)
            {
                pour.Stop();
                pour = null;
            }
        }
    }
}