using UnityEngine;

public class ForceLoopingAllParticles : MonoBehaviour
{
    public bool loop = true;

    void Awake() => Apply();
    void OnEnable() => Apply();
#if UNITY_EDITOR
    void OnValidate() => Apply();
#endif
    void Apply()
    {
        var arr = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in arr)
        {
            var m = ps.main;
            if (m.loop != loop) m.loop = loop;   // ✅ 강제
        }
    }
}
