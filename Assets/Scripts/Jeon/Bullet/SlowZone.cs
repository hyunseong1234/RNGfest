using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Model
{
    public class SlowZone : BaseObject
    {
        [SerializeField] private float _radius = 1.5f;     // 장판 영향 범위
        [SerializeField] private float _slowAmount = 0.5f; // 감속량
        [SerializeField] private BaseObject _slowEffectPrefab; // 슬로우 버프 연출용 프리팹
        private float _duration;

        public void InitZone(float duration)
        {
            _duration = duration;
            StartCoroutine(ZoneRoutine());
        }

        private IEnumerator ZoneRoutine()
        {
            float timer = 0f;
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();

            while (timer < _duration)
            {
                if (mainManager != null)
                {
                    float sqrRadius = _radius * _radius;
                    // 범위 내 적 탐색
                    var enemiesInRange = mainManager.SpawnEnemys
                        .Where(e => e != null && e.gameObject.activeSelf)
                        .Where(e => (e.transform.position - transform.position).sqrMagnitude <= sqrRadius)
                        .ToList();

                    foreach (var enemy in enemiesInRange)
                    {
                        var existingSlow = enemy.GetBuff<SlowBuff>();
                        if (existingSlow == null)
                        {
                            var slowBuff = new SlowBuff(_slowAmount);
                            // 수정된 Init 방식: 주인, 지속시간, 연출프리팹 3개를 전달
                            slowBuff.Init(enemy, 0.5f, _slowEffectPrefab);
                            enemy.AddBuff(slowBuff);
                        }
                        else
                        {
                            // 이미 버프가 있다면 시간만 0.5초로 다시 갱신
                            existingSlow.Refresh(0.5f);
                        }
                    }
                }
                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }

            // 장판 수명이 다하면 풀로 반납
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        public override void ObjectUpdate() { }
    }
}