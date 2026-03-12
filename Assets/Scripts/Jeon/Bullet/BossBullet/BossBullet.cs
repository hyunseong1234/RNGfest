using UnityEngine;
using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    // 보스 전용 스킬 탄환의 공통 분모
    public abstract class BossBullet : ParabolaBullet
    {
        protected Tower _targetTower;
        protected TileObject _targetTile;
        [SerializeField] protected BaseObject _effectPrefab; // 각 스킬의 연출 프리팹

        public virtual void InitBossSkill(Tower target, float speed)
        {
            _targetTower = target;
            _targetTile = target.CurrentTile;

            // 부모(ParabolaBullet)의 Init 호출
            base.Init(target.transform, 0f, speed);
        }

        protected override void HitTarget()
        {
            SpawnHitEffect(transform.position);

            if (_hitEffectPrefab != null && _targetTower != null && _targetTower.gameObject.activeSelf)
            {
                _targetTower.ApplyHitEffect(_hitEffectPrefab);
            }

            if (_targetTower != null && _targetTower.gameObject.activeSelf)
            {
                if (_targetTower.CurrentTile == _targetTile)
                {
                    ApplySkillEffect(_targetTower); // 여기서 얼음(Crystals crossfade 2)을 씌움
                }
            }

            ReturnToPool();
        }

        // 각 자식 스크립트(Ice, Shaman 등)에서 자신만의 효과를 구현함
        protected abstract void ApplySkillEffect(Tower targetTower);

        protected override void ReturnToPool()
        {
            base.ReturnToPool();
        }
    }
}