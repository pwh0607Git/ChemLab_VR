using UnityEngine;

public enum EPType
{
    ClockReaction,
    
    Explosion,
}


public abstract class EPReport : MonoBehaviour
{
    [SerializeField] EPType type;
    public EPType Type => type;
}