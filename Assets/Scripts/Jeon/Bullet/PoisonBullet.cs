using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class PoisonBullet : BaseBullet, IAbilityBoost
    {
        [Header("독성 설정")]
        [SerializeField] private int _poisonDamage = 5;
        [SerializeField] private float _poisonDuration = 5.0f;
        [SerializeField] private Dev.cheol.Model.BaseObject _poisonBuffVFXPrefab;

        /// <summary>
        /// 증강 적용 → 독 데미지 증가
        /// </summary>
        public void ApplyAbilityBoost(float value)
        {
            _poisonDamage = Mathf.RoundToInt(_poisonDamage * (1 + value));
            Debug.Log($"[PoisonBullet] 독 데미지 증가: {_poisonDamage}");
        }

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_target == null) return;
            var enemy = _target.GetComponent<Enemy>();
            if (enemy == null) return;

            enemy.OnDamaged(_damage, _fontColor);

            var existingPoison = enemy.GetBuff<PoisonBuff>();
            if (existingPoison != null)
            {
                if (_poisonDamage > existingPoison.Damage)
                    existingPoison.UpgradePoison(_poisonDamage);
                existingPoison.Refresh(_poisonDuration);
            }
            else
            {
                var newPoison = new PoisonBuff(_poisonDamage);
                newPoison.Init(enemy, _poisonDuration, _poisonBuffVFXPrefab);
                enemy.AddBuff(newPoison, _poisonDuration, _poisonBuffVFXPrefab);
            }
        }
    }
}