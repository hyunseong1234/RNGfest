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
                            slowBuff.Init(enemy, 0.5f); // 밟고 있는 동안은 짧은 버프를 계속 갱신
                            enemy.AddBuff(slowBuff);
                        }
                        else
                        {
                            existingSlow.Refresh(0.5f);
                        }
                    }
                }
                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }

            // 시간이 다 되면 풀로 반납
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        public override void ObjectUpdate() { }
    }
}