using System;
using System.Collections;
using System.Collections.Generic;
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
}