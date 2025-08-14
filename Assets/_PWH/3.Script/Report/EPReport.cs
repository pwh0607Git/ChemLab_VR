using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPType
{
    EP1,
    
    EP2
}


public class EPReport : MonoBehaviour
{
    [SerializeField] EPType type;
    public EPType Type => type;

    
}