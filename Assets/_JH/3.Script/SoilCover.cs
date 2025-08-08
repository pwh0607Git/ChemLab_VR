using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoilCover : MonoBehaviour
{
    public GameObject soilMesh; // 흙 Mesh 오브젝트
    public FeedPowderEmitter powderEmitter; // 1단계 스크립트 참조
    public float requiredPourTime = 2f; // 최소 기울어야 하는 시간

    private bool isCovered = false;

    public void CoverSoil()
    {
        if (isCovered) return;
        
        if(powderEmitter.pourDuration < requiredPourTime)
        {
            Debug.LogWarning($"아직 중크롬산 암모늄이 충분히 담기지 않았습니다!");
            return;
        }

        soilMesh.SetActive(true);
        isCovered = true;

        Debug.Log(" 흙으로 덮었습니다.");
    }
}
