using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ShamanBullet : BossBullet
    {
        [Header("Shaman Custom Settings")]
        [SerializeField] private float _destroyDelay = 1.0f; // 여기서 시간을 조절하세요!
        [SerializeField] private BaseObject _destroyEffectPrefab; // 1성 파괴용

        protected override void HitTarget()
        {
            // 1. 부모(BossBullet)의 SpawnHitEffect를 실행하지 않기 위해 
            // base.HitTarget()을 호출하지 않고 직접 로직을 짭니다.

            if (_targetTower != null && _targetTower.gameObject.activeSelf)
            {
                if (_targetTower.CurrentTile == _targetTile)
                {
                    ApplySkillEffect(_targetTower);
                }
            }
            ReturnToPool();
        }

        protected override void ApplySkillEffect(Tower targetTower)
        {
            // 2. 여기서 타워에게 이펙트들과 기다릴 시간을 한꺼번에 던져줍니다.
            targetTower.DowngradEffect(_effectPrefab, _destroyEffectPrefab, _destroyDelay);
        }
    }
}