using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class TowerManager : BaseManager
    {
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 타워 배치 함수
        /// </summary>
        /// <param name="selectTile"></param>
        /// <param name="getLank"> 타워의 랭크를 설정해줌</param>
        public void BuildTower(TileObject selectTile, int getLank) //타워 생성코드 클릭용
        {
            var factory = ServiceLocator.Instance.GetService<FactoryManager>();
            var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (poolManager == null)
            {
                Debug.LogError("ObjectPoolingManger를 못 찾았습니다!");
                return;
            }
            int rand = Random.Range(0, factory.Prefabs_Twoer.Length);
            var tower = poolManager.GetFromPool<Tower>(factory.Prefabs_Twoer[rand].gameObject.name);

            Debug.Log(tower);
            if (tower == null)
            {
                Debug.LogError("지으려는 타워 프리팹이 할당되지 않았습니다!");
                return;
            }

            var main = ServiceLocator.Instance.GetService<MainManager>();

            tower.transform.position = selectTile.transform.position;
            tower.CurrentTile = selectTile;
            tower.Lank = getLank; // 증강에따라 달라지는 값일 수도 있음
            main.SpawnTowers.Add(tower);
        }

        public int LankCalculator()
        {
            return 1;
        }

    }

}
