using DG.Tweening;
using UnityEngine;

public class NPCIdle : NPCState
{
    [SerializeField] float idleTime = 2f;

    public override void OnEnter()
    {
        DOVirtual.DelayedCall(idleTime, () => OnExit());
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {

    }
}
