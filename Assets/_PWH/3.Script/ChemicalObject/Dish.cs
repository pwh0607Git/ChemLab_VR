using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish : MonoBehaviour
{
    [SerializeField] private ChemFlag flag;
    public ChemFlag Flag => flag;

    [SerializeField] PoolBehaviour powder;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Spoon"))
        {
            Debug.Log("Spoon에 닿음.");
            other.GetComponent<SpoonControl>().GetItem(flag, powder);
        }
    }
}