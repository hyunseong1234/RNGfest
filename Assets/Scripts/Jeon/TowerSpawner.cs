using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefabs;

    public void SpawnTower(Transform tileTransform)
    {

        // 같은 위치에 중복 방지
        Tile tile = tileTransform.GetComponent<Tile>();

        if(tile.IsBuildTower == true)
        {
            return;
        }

        tile.IsBuildTower = true;

        // 타워 생성 나중에 오브젝트 풀링 하면 없어질것같음
        Instantiate(towerPrefabs , tileTransform.position , Quaternion.identity);
    }

}
