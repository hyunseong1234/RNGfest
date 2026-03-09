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
        [SerializeField] private List<BaseScreenUI> _spawnUI; // Ui


        [SerializeField] private Tower _selected = null;
        public List<Tower> SpawnTowers { get => _spawnTowers; set => _spawnTowers = value; }
        public List<Enemy> SpawnEnemys { get => _spawnEnemys; set => _spawnEnemys = value; }
        public Tower Selected { get => _selected; set => _selected = value; }
        public List<BaseScreenUI> SpawnUI { get => _spawnUI; set => _spawnUI = value; }


        #region 세팅 및 
        private void Start()
        {
            var factory = ServiceLocator.Instance.GetService<FactoryManager>();

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
            //UpdateList(_spawnUI);

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

        private void Test()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                var system = ServiceLocator.Instance.GetService<SystemManager>();

                if (system != null)
                {
                    system.Gold += 100000;
                }
            }
        }

        #endregion

        #region
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
        #endregion

        #region Public 함수들


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

            var towerData = factory.GetTowerData(tower.PoolTag);

            if (towerData != null)
            {
                tower.Setup(towerData, getLank);
            }
            else
            {
                Debug.LogWarning($"[Data Error] {tower.PoolTag}의 SO 데이터를 찾을 수 없습니다.");
            }

            var main = ServiceLocator.Instance.GetService<MainManager>();
            var rankManager = ServiceLocator.Instance.GetService<RankManager>();
            tower.transform.position = selectTile.transform.position;
            tower.CurrentTile = selectTile;
            main._spawnTowers.Add(tower);
            selectTile._isUsed = true; //타일 사용여부 
            rankManager.RequestRank(tower); //연결요청
        }

        /// <summary>
        /// 유닛들 리턴풀 시키는 함수 유닛들 삭제할때는 다 여기로 통해야됨
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveUnit(BaseUnit obj)
        {
            if (obj is Enemy enemy) _spawnEnemys.Remove(enemy);
            else if (obj is Tower tower)
            {
                _spawnTowers.Remove(tower);
                tower.CurrentTile._isUsed = false; // 커플링 발생 2호기

                //UI 부분 초기화 세팅 부분 위치 나중에 리팩토링 요구
                tower.StarUI.gameObject.SetActive(false);
                tower.StarUI.Target = null; //널잡아주기
                tower.StarUI = null;
            }

            // 공통 초기화 및 반납 처리
            obj.OnReturnToPool();

            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(obj);

        }
        public void TogglePause()
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                Debug.Log("게임 재개 (TimeScale: 1)");
            }
            else
            {
                Time.timeScale = 0;
                Debug.Log("게임 일시정지 (TimeScale: 0)");
            }
        }



        public override void HandleEvent(string data) { }
        #endregion
    }
}