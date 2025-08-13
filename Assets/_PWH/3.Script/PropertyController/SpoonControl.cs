using CustomInspector;
using UnityEngine;

public class SpoonControl : MonoBehaviour
{
    [SerializeField] Transform headPivot;
    [SerializeField, ReadOnly] Powder chem;
    [SerializeField] ChemFlag currentChemical;

    public void GetItem(ChemFlag flag, PoolBehaviour p)
    {
        if (currentChemical != ChemFlag.None) return;

        chem = PoolManager.Instance.Spawn(p, Vector3.zero, Quaternion.identity, headPivot) as Powder;
        currentChemical = flag;
    }

    public void RemoveChemical()
    {
        currentChemical = ChemFlag.None;
        chem = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Beaker"))
        {
            Debug.Log("비커에 닿음");

            Beaker b = other.GetComponent<Beaker>();

            b.AddPowder(currentChemical, 1);

            currentChemical = ChemFlag.None;
            chem.Despawn();

            chem = null;
        }
    }
}
