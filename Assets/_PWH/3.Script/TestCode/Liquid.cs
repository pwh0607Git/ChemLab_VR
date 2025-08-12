using UnityEngine;

public class Liquid : MonoBehaviour
{
    private bool isGrab;
    [SerializeField] private bool isPour;
    [SerializeField] private Transform head;
    VFX pour;

    void Start()
    {
        isGrab = false;
        isPour = false;
    }

    void OnDisable()
    {
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