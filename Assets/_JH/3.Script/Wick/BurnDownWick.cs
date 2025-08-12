using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BurnDownWick : MonoBehaviour
{
    [Header("타들어갈 대상")]
    public Transform wickPivot;

    [Header("불 파티클 루트(심지 끝에 자식으로 배치")]
    public Transform flameRoot;
    public bool scaleFlameWithWick = true;

    [Header("점화 소스(있으면 자동 시작 & 끝날 때 Extinguish")]
    public WickIgnitable ignitable;

    [Header("타는 연출")]
    public float burnDuration = 3.0f; // 완전히 타들어가는데 걸리는 시간
    public AnimationCurve burnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    // 내부
    Vector3 _wickInitScale;
    Vector3 _flameInitScale;
    bool _running;

    private void Awake()
    {
        if (!wickPivot) wickPivot = transform;
        if(flameRoot == null && ignitable != null)
        { 

        }
    }

    private void OnEnable()
    {
        if (ignitable != null) ignitable.onIgnited.AddListener(StartBurn);
    }

    private void OnDisable()
    {
        if (ignitable != null) ignitable.onIgnited.RemoveListener(StartBurn);
    }

    void StartBurn()
    {
        if (_running) return;
        _wickInitScale = wickPivot.localScale;
        _flameInitScale = flameRoot ? flameRoot.localScale : Vector3.one;
        StartCoroutine(BurnCo());
    }

    IEnumerator BurnCo()
    {
        _running = true;
        float t = 0f;

        while( t< burnDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / burnDuration);
            float f = 1f - burnCurve.Evaluate(k);

            wickPivot.localScale = new Vector3(_wickInitScale.x, _wickInitScale.y * f, _wickInitScale.z);

            if (scaleFlameWithWick && flameRoot)
                flameRoot.localScale = _flameInitScale * Mathf.Clamp(f, 0.001f, 1f);

            yield return null;
        }
        if(ignitable != null)
        {
            ignitable.Extinguish();
        }
        _running = false;
        
    }
}
