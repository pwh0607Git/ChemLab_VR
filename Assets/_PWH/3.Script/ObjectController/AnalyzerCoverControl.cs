using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnalyzerCoverControl : MonoBehaviour
{
    [SerializeField] List<GameObject> covers;
    [SerializeField] Vector3 targetOpen;

    public void OpenCover()
    {
        Sequence openSeq = DOTween.Sequence();
    }

    public void CloseCover()
    {
        
    }
}