using Dev.cheol.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Dev.jeon.Model
{
    public class SplitterEnemy : Enemy
    {
        [Header("분열 설정")]
        [SerializeField] private Enemy _childPrefab; // 분열되어 나올 작은 몬스터 프리팹
        private int _splitCount = 2; // 분열될 개수 (WaveManager에서 주입 가능)

        // 외부(WaveManager)에서 분열 개수를 정해줄 수 있게 함
        public int SplitCount { set => _splitCount = value; }

        protected override void OnDeath()
        {
            if (_childPrefab == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var main = ServiceLocator.Instance.GetService<MainManager>();

            for (int i = 0; i < _splitCount; i++)
            {
                // 자식 몬스터 생성
                Enemy child = pool.GetFromPool<Enemy>(_childPrefab.gameObject.name);
                if (child != null)
                {
                    // 부모의 현재 위치에서 생성 (약간의 랜덤 위치를 주면 겹치지 않음)
                    Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0, UnityEngine.Random.Range(-0.2f, 0.2f));
                    child.transform.position = transform.position + randomOffset;

                    // [중요] 부모가 가고 있던 경로 인덱스를 그대로 이어받음
                    child._waypointIndex = this._waypointIndex;

                    child.gameObject.SetActive(true);

                    // 관리 리스트에 추가
                    main.SpawnEnemys.Add(child);
                    child.RefreshPath(); // 이동 시작
                }
            }
        }
    }
}

