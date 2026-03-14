using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class FireBullet : BaseBullet
    {
        [Header("화염 스플래시 설정")]
        [SerializeField] private float _splashRadius = 1.5f;

        // 부모의 Init과 추적 로직을 그대로 사용하므로 코루틴을 직접 만들 필요가 없습니다.

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();

            // 스플래시 데미지 처리
            if (mainManager != null)
            {
                float sqrRadius = _splashRadius * _splashRadius;
                var enemiesInRange = mainManager.SpawnEnemys
                    .Where(e => e != null && e.gameObject.activeSelf)
                    .Where(e => (e.transform.position - hitPoint).sqrMagnitude <= sqrRadius)
                    .ToList();

                foreach (var enemy in enemiesInRange)
                {
                    enemy.OnDamaged(_damage, _fontColor);
                }
            }

            // 이펙트와 사운드는 부모의 OnHit에서 이미 처리되었으므로 여기선 데미지만 입힙니다.
        }
    }
}