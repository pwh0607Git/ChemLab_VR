using System;
using UnityEngine;

[Serializable]
public class PourBehaviour
{
    public Transform head;
    private VFX pourVFX;
    private bool _isOn;

    public void Initialize(Transform headTransform)
    {
        head = headTransform;
        pourVFX = VFXManager.Instance.SpawnVFX(
            VFXFlag.LiquidPour, Vector3.zero, Quaternion.identity, head, true);
        pourVFX.Stop();
        _isOn = false;
    }

    public void Start()
    {
        if (_isOn) return; // 중복 방지
        _isOn = true;
        if (pourVFX != null) pourVFX.Play();
    }

    public void Stop()
    {
        if (!_isOn) return;
        _isOn = false;
        if (pourVFX != null) pourVFX.Stop();
    }

    public void Dispose()
    {
        Stop();
        if (pourVFX != null)
        {
            pourVFX.Despawn();
            pourVFX = null;
        }
    }
}

public class Liquid : MonoBehaviour
{
    [SerializeField] ChemFlag flag;
    public ChemFlag Flag => flag;

    [SerializeField] private Transform head;

    [Header("Pour Angle")]
    public float angleThreshold = 150f;

    //[Header("Flag")]
    //private bool isGrab;

    [SerializeField] private PourBehaviour pour;

    [Header("Liquid SFX")]
    public AudioClip liquidClip;
    [SerializeField, Min(0f)] float loopFadeOut = 0.25f;
    private int _loopSfxId = 0;
    private bool _wasOn = false;

    void Start()
    {
        if (pour == null) pour = new PourBehaviour();
        if (head == null)
            Debug.LogWarning($"[Liquid] head is NULL on {name}. Assign a Transform in Inspector.");

        pour.Initialize(head);
    }

    void Update()
    {
        if (head == null) return;

        float angle = Vector3.Angle(head.up, Vector3.up);
        bool on = angle > angleThreshold;

        if (on != _wasOn)
        {
            UpdatePourState(on);
            _wasOn = on;
        }
    }

    void UpdatePourState(bool on)
    {
        if (on)
        {
            pour.Start();
            StartAudio();
        }
        else
        {
            StopAudio();
            pour.Stop();
        }
    }

    void OnDisable()
    {
        StopAudio();
        pour.Dispose();
    }

    void StartAudio()
    {
        if (liquidClip == null)
        {
            Debug.LogWarning($"[Liquid] liquidClip is NULL on {name}.");
            return;
        }
        if (SoundManager.Instance == null)
        {
            Debug.LogError("[Liquid] SoundManager.Instance is NULL. Place SoundManager in the first scene.");
            return;
        }
        if (head == null)
        {
            Debug.LogWarning($"[Liquid] head is NULL on {name}.");
            return;
        }

        if (_loopSfxId == 0)
        {
            _loopSfxId = SoundManager.Instance.PlaySFXOn(
                liquidClip, head, loop: true, volume: 1f, pitch: 1f);
            if (_loopSfxId == 0)
                Debug.LogWarning("[Liquid] PlaySFXOn returned 0 (clip or target may be invalid).");
        }
    }

    void StopAudio()
    {
        if (_loopSfxId != 0 && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSFXById(_loopSfxId, immediate: false, fadeOutSeconds: loopFadeOut);
            _loopSfxId = 0;
        }
    }
}