using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.cheol.Model
{
    // AttackTower의 모든 기능(타겟팅, 발사 등)을 그대로 상속받습니다.
    public class SkillTower : AttackTower
    {
        [Header("주기적 스킬 설정")]
        [SerializeField] private BaseObject _skillPrefab;      // 소환할 프리팹 (장판 등)
        [SerializeField] private float _skillInterval = 5.0f;  // 스킬 발동 주기 (초)
        [SerializeField] private int _spawnAmount = 2;         // 한 번에 소환할 개수
        [SerializeField] private float _objectDuration = 4.0f; // 소환된 객체의 유지 시간

        private float _skillTimer = 0f;

        public override void ObjectUpdate()
        {
            // 1. 기존 AttackTower의 로직(타겟 찾기, 공격 상태 전환 등) 실행
            base.ObjectUpdate();

            // 2. 독립적인 스킬 타이머 작동
            UpdateSkillTimer();
        }

        private void UpdateSkillTimer()
        {
            _skillTimer += Time.deltaTime;

            if (_skillTimer >= _skillInterval)
            {
                _skillTimer = 0f;
                ExecuteSkill();
            }
        }

        private void ExecuteSkill()
        {
            var mapManager = ServiceLocator.Instance.GetService<MapManager>();
            var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (mapManager == null || poolManager == null || _skillPrefab == null) return;

            Transform[] path = mapManager.FlagPoints;
            if (path == null || path.Length < 2) return;

            // 설정된 개수만큼 랜덤한 길 위에 소환
            for (int i = 0; i < _spawnAmount; i++)
            {
                int segment = Random.Range(0, path.Length - 1);
                float t = Random.Range(0f, 1f);
                Vector3 spawnPos = Vector3.Lerp(path[segment].position, path[segment + 1].position, t);

                BaseObject skillObj = poolManager.GetFromPool<BaseObject>(_skillPrefab);
                if (skillObj != null)
                {
                    skillObj.transform.position = spawnPos;
                    skillObj.gameObject.SetActive(true);

                    // 소환된 객체가 SlowZone처럼 지속 시간이 필요한 경우 초기화
                    // 만약 다른 종류의 객체라면 해당 클래스에 맞춰 형변환 하면 됩니다.
                    if (skillObj is SlowZone slowZone)
                    {
                        slowZone.InitZone(_objectDuration);
                    }
                }
            }

            // Debug.Log($"<color=yellow>[SkillTower]</color> 주기적 스킬 발동: {_spawnAmount}개 소환 완료.");
        }
    }
}