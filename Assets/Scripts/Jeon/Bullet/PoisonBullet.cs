using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class PoisonBullet : BaseBullet
    {
        [Header("독성 설정")]
        [SerializeField] private int _poisonDamage = 5;
        [SerializeField] private float _poisonDuration = 5.0f;
        [SerializeField] private BaseObject _poisonBuffVFXPrefab;

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_target == null) return;

            var enemy = _target.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 1. 즉발 데미지
                enemy.OnDamaged(_damage, _fontColor);

                // 2. 독 버프 로직
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
}