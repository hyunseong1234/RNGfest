using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dev.cheol.Manager;

namespace Dev.jeon.Manager
{
    public class WaveManager : BaseManager
    {
        [SerializeField] private List<WaveData> _waves;

        private int _currentWaveIndex = 0;
        private void Update()
        {
            // F2 키를 누르면 다음 웨이브 시작!
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log("웨이브 시작 버튼(F3) 눌림!");
                StartNextWave();
            }
        }
        public void StartNextWave()
        {
            if (_currentWaveIndex < _waves.Count)
            {
                StartCoroutine(WaveRoutine(_waves[_currentWaveIndex]));
                _currentWaveIndex++;
            }
            else
            {
                Debug.Log("모든 웨이브가 끝났습니다!");
            }
        }
        private IEnumerator WaveRoutine(WaveData data)
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var map = ServiceLocator.Instance.GetService<MapManager>();
            var main = ServiceLocator.Instance.GetService<MainManager>();

            // 1. 퐁당퐁당 소환을 위해 모든 몬스터를 리스트에 담기
            List<Enemy> allMonsters = new List<Enemy>();
            foreach (var info in data.monsterTypes)
            {
                for (int i = 0; i < info.count; i++)
                {
                    allMonsters.Add(info.monsterPrefab);
                }
            }

            // 2. 리스트 무작위로 섞기 (셔플)
            for (int i = 0; i < allMonsters.Count; i++)
            {
                int rnd = Random.Range(i, allMonsters.Count);
                Enemy temp = allMonsters[i];
                allMonsters[i] = allMonsters[rnd];
                allMonsters[rnd] = temp;
            }

            // 3. 섞인 몬스터들을 순서대로 소환
            foreach (var prefab in allMonsters)
            {
                Enemy monster = pool.GetFromPool<Enemy>(prefab.gameObject.name);

                if (monster != null)
                {
                    main.SpawnEnemys.Add(monster);

                    // 🔥 형님 방식 적용: 스폰 위치를 첫 번째 깃발(경로) 위치로 지정!
                    monster.transform.position = map.FlagPoints[0].position;

                    monster.RefreshPath(); // 다음 경로로 이동 시작
                }

                // 소환 간격 대기 (일단 0.5초)
                yield return new WaitForSeconds(0.5f);
            }

            // 4. 보스 소환 로직
            if (data.hasBoss && data.bossPrefab != null)
            {
                Enemy boss = pool.GetFromPool<Enemy>(data.bossPrefab.gameObject.name);
                if (boss != null)
                {
                    main.SpawnEnemys.Add(boss);
                    boss.transform.position = map.FlagPoints[0].position;
                    boss.RefreshPath();
                }
            }
        }

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

    }
}
