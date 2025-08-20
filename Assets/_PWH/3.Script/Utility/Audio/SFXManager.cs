using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomInspector;


public enum ClipFlag
{
    Test1,
}

[System.Serializable]
public struct ClipData
{
    public ClipFlag flag;
    public AudioClip clip;
}
public class SFXManager : BehaviourSingleton<SFXManager>
{
    protected override bool IsDontDestroy() => false;

    [SerializeField] List<ClipData> clips;
    [SerializeField] SFX sfx;               // sfx 모델

    public SFX SpawnSFX(ClipFlag flag, Vector3 position, Transform parent, float spatial = 0.9f)
    {
        ClipData data = clips.Find(c => c.flag.Equals(flag));

        SFX sfxInstance = Instantiate(sfx, parent);
        sfx.SetProperty(data.clip, position, spatial);

        return sfx;
    }

    [Button("TestSpawnSFX"), HideField] public bool b1;
    public void TestSpawnSFX()
    {
        SpawnSFX(ClipFlag.Test1, Vector3.zero, null);
    }
}   