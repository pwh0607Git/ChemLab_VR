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

    public void Stop()
    {
        if (particle.isStopped) return;
        particle.Stop();
        Despawn();
    }

    void Update()
    {
        // 단발성 
        if (particle.isStopped)
        {
            Despawn();
        }
    }
}