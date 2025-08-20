using UnityEngine;

public abstract class PoolBehaviour : MonoBehaviour
{
    public void Despawn() => PoolManager.Instance.Despawn(this);
}