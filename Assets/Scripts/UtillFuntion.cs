using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Utill
{
    public static class UtillFuntion
    {
        public static Vector3 RandTileFind(List<TileObject> tiles)
        {
            Vector3 tempVec = new Vector3();
            var availableTiles = tiles.Where(t => !t._isUsed).ToList();

            // 이후 로직 (예: 이 중에서 랜덤으로 하나 뽑기)
            if (availableTiles.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTiles.Count);
                TileObject selectedTile = availableTiles[randomIndex];
                tempVec = selectedTile.transform.position;
            }

            return tempVec;

        }

        public static Vector3 RandTileFind(TileObject[] tiles)
        {
            Vector3 tempVec = Vector3.zero;
            var availableTiles = tiles.Where(t => !t._isUsed).ToList();

            // 이후 로직 (예: 이 중에서 랜덤으로 하나 뽑기)
            if (availableTiles.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTiles.Count);
                TileObject selectedTile = availableTiles[randomIndex];
                availableTiles[randomIndex]._isUsed = true;

                tempVec = selectedTile.transform.position;
            }
            else
            {
                Debug.Log("버그임? or 잡혀져있는 객체가 없음 _isUsed 확인 요망");
            }
            Debug.Log("현재 잡힌 벡터값" + tempVec);
            return tempVec;
        }


    }


}
