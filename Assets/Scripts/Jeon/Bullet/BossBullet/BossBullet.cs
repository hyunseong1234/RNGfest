using UnityEngine;
using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    public abstract class BossBullet : ParabolaBullet
    {
        protected Tower _targetTower;
        protected TileObject _targetTile;
        protected int _targetInitialLank;

        // 보스 스킬에서 공통으로 사용할 이펙트 프리팹 (얼음, 저주 등)
        [SerializeField] protected BaseObject _effectPrefab;

        public virtual void InitBossSkill(Tower target, float speed)
        {
            _targetTower = target;
            _targetTile = target.CurrentTile;

            base.Init(target.transform, 0f, speed);
        }

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_targetTower != null && _targetTower.gameObject.activeInHierarchy)
            {
                if (_targetTower.CurrentTile == _targetTile)
                {
                    ApplySkillEffect(_targetTower);
                }
            }
        }

        // 각 자식(Ice, Shaman)이 구현할 구체적인 스킬 효과
        protected abstract void ApplySkillEffect(Tower targetTower);
    }
}