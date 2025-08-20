using UnityEngine;

public class VFX : PoolBehaviour
{
    [SerializeField] ParticleSystem particle;

    void Start()
    {
        particle = GetComponentInChildren<ParticleSystem>();
    }

    public void SetLoop(bool on)
    {
        var p_main = particle.main;
        p_main.loop = on;
    }

    void OnEnable()
    {
        particle.Play();
    }

    public void Play()
    {
        if (particle.isPlaying) return;
        gameObject.SetActive(true);
        particle.Play();
    }

    // 주로 Loop에서 멈출때 사용
    public void Stop()
    {
        if (particle.isStopped) return;
        particle.Stop();
    }
}