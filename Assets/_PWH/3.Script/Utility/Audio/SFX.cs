using UnityEngine;

public class SFX : PoolBehaviour
{
    [SerializeField] AudioSource audio;

    void Start()
    {
        TryGetComponent(out audio);
    }

    public void SetProperty(AudioClip clip, Vector3 position, float spatial)
    {
        audio.clip = clip;
        audio.spatialBlend = spatial;
        gameObject.transform.position = position;
        audio.Play();
    }

    public void Play()
    {
        audio.Play();
    }

    public void Stop()
    {
        audio.Stop();
    }

    // 단발성.
    void Update()
    {
        if (!audio.isPlaying)
        {
            Debug.Log($"{gameObject.name} : Stop");
            Despawn();
        }
    }
}
