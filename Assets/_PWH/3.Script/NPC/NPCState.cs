using System;
using UnityEngine;

[Serializable]
public abstract class NPCState : MonoBehaviour
{
    [SerializeField] State state;

    public State State => state;

    protected NPCController npc;

    public void InitState(NPCController npc)
    {
        this.npc = npc;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}