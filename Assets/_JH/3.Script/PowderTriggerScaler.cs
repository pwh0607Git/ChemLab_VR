using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowderTriggerScaler : MonoBehaviour
{
    public Transform heap;
    public float secondsForFull = 5f;
    public Vector2 radiusRange = new(0.05f, 0.12f);
    public Vector2 heightRange = new(0.01f, 0.06f);
    public float smooth = 8f;

    [Header("커버 허용 임계치 (0~1)")]
    [Range(0,1)] public float coverThreshold01 = 0.3f;
    public UnityEvent onCoverReady; // (선택) 임계치 도달 시 1회 호출

    private ParticleSystem ps;
    private readonly List<ParticleSystem.Particle> inside = new();
    private readonly List<ParticleSystem.Particle> enter  = new();
    private float accum;             // PourZone 안에서 누적된 시간(초)
    private bool firedEvent = false; // onCoverReady 1회 발사 제어
    private Vector3 initScale, initPos;

    // 외부에서 읽을 값들
    public float AccumulatedSeconds => accum;
    public float Fill01 => secondsForFull <= 0f ? 1f : Mathf.Clamp01(accum / secondsForFull);
    public bool ReadyToCover => Fill01 >= coverThreshold01;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (!heap) { Debug.LogError("[PowderTriggerScaler] heap not set"); enabled = false; return; }
        initScale = heap.localScale;
        initPos   = heap.localPosition;
    }

    void Update()
    {
        // 스케일 보간
        float t = Fill01;
        float r = Mathf.Lerp(radiusRange.x, radiusRange.y, t);
        float h = Mathf.Lerp(heightRange.x, heightRange.y, t);
        Vector3 target = new(r, h, r);

        heap.localScale = Vector3.Lerp(heap.localScale, target, Time.deltaTime * smooth);
        float offsetY = (heap.localScale.y - initScale.y) * 0.5f;
        heap.localPosition = initPos + new Vector3(0, offsetY, 0);

        if (!firedEvent && ReadyToCover)
        {
            firedEvent = true;
            onCoverReady?.Invoke(); // 필요하면 SoilCover.EnableButton 같은 걸 연결
        }
    }

    void OnParticleTrigger()
    {
        int nIn = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);
        int nEn = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter,  enter);

        if (nIn > 0 || nEn > 0)
            accum = Mathf.Min(accum + Time.deltaTime, secondsForFull);
    }

    public void ResetAccumulation()
    {
        accum = 0f; firedEvent = false;
        heap.localScale = initScale;
        heap.localPosition = initPos;
    }
}
