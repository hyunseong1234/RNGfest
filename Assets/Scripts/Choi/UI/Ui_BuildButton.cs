using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Utill;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ui_BuildButton : UIObject
{

    /// <summary>
    /// 나중에 각 매니저만 호출 해서 할수있도록
    /// </summary>
    public void _BuildTewor()
    {

        //버튼 흔들리는 연출 넣어줘용
        var system = ServiceLocator.Instance.GetService<SystemManager>();

        Debug.Log("타워 생성 호출");
        var TileManager = ServiceLocator.Instance.GetService<TileManager>();
        var availableTiles = TileManager.MapTile.Where(t => !t._isUsed).ToList();

        if (availableTiles.Count <= 0) return; // 아래부분에 돈 소모 로직 넣으면 된다. 경고 위에 나뚜면 디진다.

        int needGold = 10 + (system.BuildCount * 10);
        if (system.Gold < needGold)
        {
            Debug.Log("돈이 없어용 ㅜㅜ");
            return;
        }
        system.Gold -= needGold;
        system.BuildCount++;

        TileObject selectTile = UtillFuntion.RandTileFind(TileManager.MapTile);

        if (selectTile == null) return;

        //증강 고려
        ServiceLocator.Instance.GetService<TowerManager>().BuildTower(selectTile, 1);
    }


}
