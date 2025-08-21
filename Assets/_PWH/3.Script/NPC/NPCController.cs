using System.Collections.Generic;
using CustomInspector;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
    Idle,
    Wandor,
}

public class NPCController : MonoBehaviour
{

    [Header("NPC Props")]
    public float moveSpeed = 15f;
    
    public NavMeshAgent agent;
    [SerializeField, ReadOnly] NPCState currentState;
    [SerializeField] List<NPCState> states;
    Dictionary<State, NPCState> stateDic;

    void Start()
    {
        TryGetComponent(out agent);

        InitDic();

        ChangeState(State.Idle);
    }

    void InitDic()
    {
        foreach (var s in states)
        {
            stateDic.Add(s.State, s);
            stateDic[s.State].InitState(this);
        }
    }

    void Update()
    {
        currentState.OnUpdate();
    }

    void ChangeState(State state)
    {
        this.currentState = stateDic[state];

        stateDic[state].OnEnter();
    }
}
