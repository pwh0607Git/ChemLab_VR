using UnityEngine;

public class FeedPowderEmitter : MonoBehaviour
{
    [Header("가루 파티클")]
    public ParticleSystem powderParticle;

    [Header("기울기 임계값")]
    public float angleThreshold = 60f;

    [Header("쏟은 시간")]
    public float pourDuration = 0f;

    private bool isPouring = false;
    public bool IsPouring => isPouring; // 외부에서 읽을 수 있는 프로퍼티

    void Update()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > angleThreshold)
        {
            if (!isPouring)
            {
                if (!powderParticle.isPlaying)
                    powderParticle.Play();

                isPouring = true;
            }
            pourDuration += Time.deltaTime;
        }
        else if (angle <= angleThreshold && isPouring)
        {
            powderParticle.Stop();
            isPouring = false;
        }
    }
}
