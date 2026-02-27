using Dev.cheol.Model;
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

        public static Vector3 RandTileFindVector(TileObject[] tiles)
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


        public static TileObject RandTileFind(TileObject[] tiles)
        {
            if (tiles == null || tiles.Length == 0)
            {
                Debug.LogWarning("RandTileFind: 타일 배열이 비어있습니다.");
                return null;
            }

            // 사용 중이지 않은 타일만 필터링
            var availableTiles = tiles.Where(t => t != null && !t._isUsed).ToList();

            // 사용중인 타일이 없는경우 랜덤 돌리는 로직 부분
            if (availableTiles.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTiles.Count);
                TileObject selectedTile = availableTiles[randomIndex];


                selectedTile._isUsed = true;

                Debug.Log($"[RandTileFind] 선택된 타일: {selectedTile.name} | 좌표: {selectedTile.transform.position}");
                return selectedTile;
            }

            // 3. 가용한 타일이 없는 경우
            Debug.LogWarning("RandTileFind: 모든 타일이 사용 중이거나 조건에 맞는 타일이 없습니다.");
            return null;
        }


    }


}
