using UnityEngine;

public class FeedPowderEmitter : MonoBehaviour
{
    [Header("가루 파티클")]
    public ParticleSystem powderParticle;

    [Header("가루 SFX")]
    public AudioClip powderClip;
    [SerializeField, Min(0f)] float loopFadeOut = 0.25f;
    private int _loopSfxId = 0; // 루프 사운드 인스턴스 ID

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
                {
                    powderParticle.Play();
                    StartPowderAudio();
                }

                isPouring = true;
            }
            pourDuration += Time.deltaTime;
        }
        else if (angle <= angleThreshold && isPouring)
        {
            powderParticle.Stop();
            StopPowderAudio();
            isPouring = false;
        }
    }

    void StartPowderAudio()
    {
        if (_loopSfxId == 0 && powderClip)
            _loopSfxId = SoundManager.Instance.PlaySFXOn(powderClip, transform, loop: true, volume: 1f, pitch: 1f);
    }

    void StopPowderAudio()
    {
        // 루프 사운드만 부드럽게 페이드아웃
        if (_loopSfxId != 0)
        {
            SoundManager.Instance.StopSFXById(_loopSfxId, immediate: false, fadeOutSeconds: loopFadeOut);
            _loopSfxId = 0;
        }
        // (안전망) 혹시 남아있을 수 있는 동일 클립 일괄 정지 원하면 아래 주석 해제
        if (powderClip) SoundManager.Instance.StopSFX(powderClip, immediate: true);
    }
}