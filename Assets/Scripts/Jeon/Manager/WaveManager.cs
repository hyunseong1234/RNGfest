using Dev.cheol.Manager;
using Dev.jeon.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

namespace Dev.jeon.Manager
{
    public class WaveManager : BaseManager
    {
        [SerializeField] private List<WaveData> _waves;
        [SerializeField] private float _spawnDelay = 0.5f;

        [SerializeField] private WaveUIController _waveUI;
        [SerializeField] private WavePopupUI _wavePopup;

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

            LoadWaveResources();
            Debug.Log("게임 시작! 첫 웨이브를 자동으로 호출합니다.");
            StartNextWave();
        }

        /// <summary>
        /// 데이터 불러오기
        /// </summary>
        private void LoadWaveResources()
        {
            // 1. Resources/Waves 폴더에 있는 모든 WaveData 로드
            WaveData[] loadedWaves = Resources.LoadAll<WaveData>("Data/Waves");

            if (loadedWaves.Length == 0)
            {
                Debug.LogError("Resources/Waves 폴더에 WaveData가 없습니다! 컨버터를 먼저 돌리세요.");
                return;
            }

            // 2. 리스트에 담기
            _waves = new List<WaveData>(loadedWaves);

            // 3. 이름순으로 정렬 (Wave_01, Wave_02 순서대로 진행하기 위함)
            _waves.Sort((a, b) => string.Compare(a.name, b.name));

            Debug.Log($"{_waves.Count}개의 웨이브 데이터를 성공적으로 로드했습니다.");
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
            // 웨이브 실행 UI

            // 1. 상시 UI 업데이트 (단순 숫자)
            if (_waveUI != null) _waveUI.ShowWave(_currentWaveIndex);

            // 2. 2초 팝업 연출 실행 (보스면 DANGER, 아니면 WAVE X)
            if (_wavePopup != null) _wavePopup.PlayPopup(_currentWaveIndex, data.waveType);


            // 몬스터 웨이브 대기 시간
            if (data.delayBeforeWave > 0)
            {
                Debug.Log($"{data.waveName} 시작 전 {data.delayBeforeWave}초 대기 중...");
                yield return new WaitForSeconds(data.delayBeforeWave);
            }

            Debug.Log($"{data.waveName} 몬스터 스폰 시작!");

            if (data.waveType == WaveType.Boss)
            {
                // 보스 스폰 (보스 전용 스탯 전달)
                if (data.bossPrefab != null)
                {
                    SpawnEntity(data.bossPrefab, data.bossHp, data.bossGoldReward);
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
                            SpawnEntity(info.monsterPrefab, info.hpOverride, info.goldReward);
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

        private void SpawnEntity(Enemy prefab, float hpOverride, int goldReward)
        {
            Enemy entity = _pool.GetFromPool<Enemy>(prefab.gameObject.name);
            if (entity != null)
            {
                // 스탯 주입 (hpOverride가 0보다 클 때만 적용)
                if (hpOverride > 0)
                {
                    if (entity.PoolTag == "Stone")
                    {
                        entity._stat.MaxHp.BaseValue = hpOverride * 1.5f;
                        entity._stat.CurrentHp = hpOverride * 1.5f;
                    }
                    else
                    {

                        entity._stat.MaxHp.BaseValue = hpOverride;
                        entity._stat.CurrentHp = hpOverride;
                    }
                }

                // 골드 보상 설정
                entity.GetGold = goldReward;


                float speed = 1;

                //현재 기획상 시트에서 몬스터 SO에서 스탯 관리 따로 안하기때문에 클라이언트 처리
                switch (entity.PoolTag)
                {
                    case "Speed":
                        speed = 1.3f;
                        break;
                    case "Stone":
                    case "Branch":
                        speed = 1f * 0.7f;
                        break;
                    case "Normal":
                        speed = 1f;
                        break;
                    default:
                        break;
                }
                entity._stat.Speed.BaseValue = speed;

                // 위치 설정 및 관리 리스트 추가
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
        }
    }
}