using System;
using System.Collections;
using System.Collections.Generic;
using CustomInspector;
using Unity.VisualScripting;
using UnityEngine;

public enum VFXFlag
{
    LiquidPour,
    Pot,
    FlameFx,
    FlameFx2,
    Flame,
    Smoke,
    Ash
}

[Serializable]
public struct VFXData
{
    public VFXFlag flag;
    public VFX vfx;
}

public class VFXManager : BehaviourSingleton<VFXManager>
{
    protected override bool IsDontDestroy() => true;

    [Header("VFX")]
    [SerializeField] List<VFXData> vfxes;

    public VFX SpawnVFX(VFXFlag flag, Vector3 position, Quaternion rot, Transform parent, bool isLoop = false)
    {
        VFX vfx = vfxes.Find(v => v.flag.Equals(flag)).vfx;

        if (vfx == null) return null;

        var returnVfx = PoolManager.Instance.Spawn(vfx, position, rot, parent);

        vfx.SetLoop(isLoop);

        return returnVfx as VFX;
    }


    [Header("VFX 출력 Tester")]
    public VFXFlag testFlag;
    [Space2(20)]
    [Button("TestSpawnVFX"), HideField] public bool b1;
    [Button("StopTestVFX"), HideField] public bool b2;
    [Space2(20)]
    
    [SerializeField, ReadOnly] private VFX testVFX;

    public void TestSpawnVFX()
    {
        if (testVFX != null)
        {
        testVFX.Stop();
            testVFX = null;
        }

        testVFX = SpawnVFX(testFlag, transform.position, Quaternion.identity, transform, false);
    }


    public void StopTestVFX()
    {
        if (testVFX == null) return;
        testVFX.Stop();
    }
}