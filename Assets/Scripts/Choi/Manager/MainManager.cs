using Dev.cheol.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dev.cheol.Manager
{
    public class MainManager : BaseManager
    {
        [Header("Object Lists")]
        [SerializeField] private List<Enemy> _spawnEnemys; //기본적으로 사용되는 에너미들 객체용
        [SerializeField] private List<Tower> _spawnTowers; // 플레이어들의 타워

        [SerializeField] private Tower _selected = null;
        public List<Tower> SpawnTowers { get => _spawnTowers; set => _spawnTowers = value; }
        public List<Enemy> SpawnEnemys { get => _spawnEnemys; set => _spawnEnemys = value; }

        private void Start()
        {
            var factory = ServiceLocator.Instance.GetService<FactoryManager>();
            //OpjSetting(factory.Prefabs_Enmey, 8);
            //OpjSetting(factory.Prefabs_Twoer, 20);


            StartCoroutine(WaitForSeedAndSpawn());
        }



        /// <summary>
        /// 초반 세팅용 코루틴 함수 네트워크 염두
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator WaitForSeedAndSpawn()
        {

            yield return new WaitForSeconds(0.2f);

        }

        private void Update()
        {

            //업데이트
            UpdateList(_spawnEnemys);
            UpdateList(_spawnTowers);
            //테스트용 인풋기능
            Test(); // 테스트때만 주석풀어용~


            //업데이트 매니저 업데이트 호출해주는 구간
            if (ServiceLocator.Instance.UpdateManagers == null) return;
            if (ServiceLocator.Instance.UpdateManagers.Count <= 0) return;

            foreach (var manager in ServiceLocator.Instance.UpdateManagers)
            {
                manager.ManagerUpdate();
            }
        }

        private void UpdateList<T>(List<T> ts) where T : BaseObject
        {

            if (ts == null || ts.Count <= 0) return;
            for (int i = ts.Count - 1; i >= 0; i--)
            {
                if (ts[i] == null) //그럴리가 없으나 혹시라도 순회돌 대상이 리무브만되고 리스트값이 있는경우가 있을때
                {
                    ts.RemoveAt(i); // 예외처리
                    continue;
                }
                ts[i].ObjectUpdate();
            }

        }

        /// <summary>
        /// 유닛들 리턴풀 시키는 함수 유닛들 삭제할때는 다 여기로 통해야됨
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveUnit(BaseUnit obj)
        {
            if (obj is Enemy enemy) _spawnEnemys.Remove(enemy);
            else if (obj is Tower tower) _spawnTowers.Remove(tower);

            // 공통 초기화 및 반납 처리
            obj.OnReturnToPool();
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(obj);
        }

        private void Test()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("몬스터 생성");
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                var monster = ServiceLocator.Instance.GetService<ObjectPoolingManger>().GetFromPool<Enemy>(factory.Prefabs_Enmey[0].gameObject.name);

                var mapmanager = ServiceLocator.Instance.GetService<MapManager>();
                _spawnEnemys.Add(monster);
                monster.transform.position = mapmanager.FlagPoints[0].position;
            }

            //돈 주는 치트키
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var system = ServiceLocator.Instance.GetService<SystemManager>();
                system.Gold += 100000;
            }
        }

        public override void HandleEvent(string data) { }
    }
}