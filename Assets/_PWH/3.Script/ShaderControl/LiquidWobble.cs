using UnityEditor.Search;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LiquidWobble : MonoBehaviour
{
    public float maxWobble = 0.03f;
    [Range(0,5), SerializeField] float wobbleSpeed = 1f;
    [Range(0,5), SerializeField] float recovery = 1f;
    private float wobbleAmountX;
    private float wobbleAmountZ;

    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;

    private float pulse;
    private float time = 0.5f;

    Vector3 velocity;
    Vector3 angularVelocity;

    private Vector3 lastPos;
    private Vector3 lastRot;

    [SerializeField] private Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend == null)
        {
            Debug.Log("Renderer is null");

            return;
        }
        time += Time.deltaTime;

        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * recovery);

        pulse = 2 * Mathf.PI * wobbleSpeed;

        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);
        
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        rend.material.SetFloat("_WobbleZ", wobbleAmountZ);

        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        // 상태 업데이트
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
    }
}
