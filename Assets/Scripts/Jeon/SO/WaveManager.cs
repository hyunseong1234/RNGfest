using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dev.cheol.Manager;

namespace Dev.jeon.Manager
{
    public class WaveManager : BaseManager
    {
        [SerializeField] private List<WaveData> _waves;
        [SerializeField] private float _spawnDelay = 0.5f;

        private int _currentWaveIndex = 0;
        private bool _isGameOver = false;

        private ObjectPoolingManger _pool;
        private MapManager _map;
        private MainManager _main;

        private void Start()
        {
            _pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            _map = ServiceLocator.Instance.GetService<MapManager>();
            _main = ServiceLocator.Instance.GetService<MainManager>();

            Debug.Log("게임 시작! 첫 웨이브를 자동으로 호출합니다.");
            StartNextWave();
        }

        public void StartNextWave()
        {
            if (_isGameOver) return;

            if (_currentWaveIndex < _waves.Count)
            {
                WaveData nextWave = _waves[_currentWaveIndex];
                _currentWaveIndex++;

                StartCoroutine(WaveRoutine(nextWave));
            }
            else
            {
                Debug.Log("모든 웨이브가 끝났습니다!");
            }
        }

        private IEnumerator WaveRoutine(WaveData data)
        {
            // 몬스터 웨이브 대기 시간
            if (data.delayBeforeWave > 0)
            {
                Debug.Log($"{data.waveName} 시작 전 {data.delayBeforeWave}초 대기 중...");
                yield return new WaitForSeconds(data.delayBeforeWave);
            }

            Debug.Log($"{data.waveName} 몬스터 스폰 시작!");

            if (data.waveType == WaveType.Boss)
            {
                if (data.bossPrefab != null)
                {
                    SpawnEntity(data.bossPrefab);
                }
            }
            else
            {
                foreach (var info in data.monsterTypes)
                {
                    for (int i = 0; i < info.count; i++)
                    {
                        if (_isGameOver) yield break;

                        if (info.monsterPrefab != null)
                        {
                            SpawnEntity(info.monsterPrefab);
                        }

                        yield return new WaitForSeconds(_spawnDelay);
                    }
                }
            }

            yield return new WaitUntil(() => _main.SpawnEnemys.Count == 0);

            if (!_isGameOver)
            {
                Debug.Log($"{data.waveName} 클리어! 다음 웨이브를 시작합니다.");
                StartNextWave();
            }
        }

        private void SpawnEntity(Enemy prefab)
        {
            Enemy entity = _pool.GetFromPool<Enemy>(prefab.gameObject.name);
            if (entity != null)
            {
                _main.SpawnEnemys.Add(entity);
                entity.transform.position = _map.FlagPoints[0].position;
                entity.RefreshPath();
            }
        }

        // TODO : 추후 다른 매니저에서 관리 하는 순간 삭제 할것
        public void GameOver()
        {
            _isGameOver = true;
            StopAllCoroutines();
            Debug.Log("게임 오버! 몬스터 스폰을 중지합니다.");
        }

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}