using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ShamanBullet : BossBullet
    {
        [SerializeField] private BaseObject _destroyEffectPrefab; // 1¼º ÆÄ±«¿ë
        protected override void ApplySkillEffect(Tower targetTower)
        {
            targetTower.DowngradEffect(_effectPrefab, _destroyEffectPrefab);
        }
    }
}