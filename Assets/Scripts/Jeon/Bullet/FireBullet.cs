using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class FireBullet : BaseBullet, IAbilityBoost
    {
        [Header("화염 스플래시 설정")]
        [SerializeField] private float _splashRadius = 1.5f;

        /// <summary>
        /// 증강 적용 → 스플래시 범위 증가
        /// </summary>
        public void ApplyAbilityBoost(float value)
        {
            _splashRadius *= (1 + value);
            Debug.Log($"[FireBullet] 스플래시 범위 증가: {_splashRadius}");
        }

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager == null) return;

            float sqrRadius = _splashRadius * _splashRadius;
            var enemiesInRange = mainManager.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf)
                .Where(e => (e.transform.position - hitPoint).sqrMagnitude <= sqrRadius)
                .ToList();

            foreach (var enemy in enemiesInRange)
                enemy.OnDamaged(_damage, _fontColor);
        }
    }
}