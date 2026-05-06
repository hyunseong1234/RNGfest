using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class SlowBullet : BaseBullet, IAbilityBoost
    {
        [SerializeField] private float _slowAmount = 0.5f;
        [SerializeField] private float _slowDuration = 2.0f;

        /// <summary>
        /// 증강 적용 → 슬로우 수치 증가
        /// </summary>
        public void ApplyAbilityBoost(float value)
        {
            _slowAmount = Mathf.Clamp(_slowAmount + value, 0f, 0.9f); // 최대 90% 슬로우
            Debug.Log($"[SlowBullet] 슬로우 수치 증가: {_slowAmount}");
        }

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_target != null && _target.TryGetComponent(out Enemy enemy))
            {
                enemy.OnDamaged(_damage, _fontColor);
                var existingSlow = enemy.GetBuff<SlowBuff>();
                if (existingSlow == null)
                {
                    var slowBuff = new SlowBuff(_slowAmount);
                    slowBuff.Init(enemy, _slowDuration, _hitEffectPrefab);
                    enemy.AddBuff(slowBuff, _slowDuration, _hitEffectPrefab);
                }
                else existingSlow.Refresh(_slowDuration);
            }
        }
    }
}