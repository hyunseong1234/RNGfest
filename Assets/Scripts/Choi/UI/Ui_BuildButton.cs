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
        var tower = ServiceLocator.Instance.GetService<ObjectPoolingManger>().GetFromPool<Tower>("Tower1");
        var main = ServiceLocator.Instance.GetService<MainManager>();
        tower.transform.position = selectTile;
        main.SpawnTowers.Add(tower);
    }
}
