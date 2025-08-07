using UnityEngine;

public class FeedPowderEmitter : MonoBehaviour
{
    [Header("가루 파티클")]
    public ParticleSystem powderParticle;

    [Header("기울기 임계값")]
    public float angleThreshold = 60; // 몇도 이상 기울이면 쏟아짐

    [Header("쏟은 시간")]
    public float pourDuration = 0f;

    private bool isPouring = false;

    private void Update()
    {
        // 현재 오브젝트의 위쪽 방향과 월드 위쪽 방향 간의 각도
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
