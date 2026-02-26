using Dev.cheol.Manager;
using Dev.cheol.Utill;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ui_BuildButton : UIObject
{
    public void _BuildTewor()
    {

        Debug.Log("타워 생성 호출");
        var TileManager = ServiceLocator.Instance.GetService<TileManager>();
        var availableTiles = TileManager.MapTile.Where(t => !t._isUsed).ToList();

        if (availableTiles.Count <= 0) return; // 아래부분에 돈 소모 로직 넣으면 된다. 경고 위에 나뚜면 디진다.

        Vector3 selectTile = UtillFuntion.RandTileFind(TileManager.MapTile);

        if (selectTile == Vector3.zero) return;

        //Todo : 원래 위치 찾아줘야됨
        BuildTower(selectTile);
    }

    /// <summary>
    /// Todo : 코드의 원래 위치 찾아줘야됨
    /// </summary>
    /// <param name="selectTile"></param>
    private void BuildTower(Vector3 selectTile) //타워 생성코드 클릭용
    {
        var factory = ServiceLocator.Instance.GetService<FactoryManager>();

        var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

        // 1번 체크: 매니저가 있나?
        if (poolManager == null)
        {
            Debug.LogError("ObjectPoolingManger를 못 찾았습니다!");
            return;
        }

        var tower = poolManager.GetFromPool<Tower>(factory.Prefabs_Twoer[0].gameObject.name);

        Debug.Log(tower);
        // 2번 체크: 지으려는 타워 정보가 있나? (변수명은 형님 코드에 맞춰보세요)
        if (tower == null)
        {
            Debug.LogError("지으려는 타워 프리팹이 할당되지 않았습니다!");
            return;
        }



        var main = ServiceLocator.Instance.GetService<MainManager>();
        tower.transform.position = selectTile;
        main.SpawnTowers.Add(tower);
    }
}
