using UnityEngine;
using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    public abstract class BossBullet : ParabolaBullet
    {
        protected Tower _targetTower;
        protected TileObject _targetTile;

        // 보스 스킬에서 공통으로 사용할 이펙트 프리팹 (얼음, 저주 등)
        [SerializeField] protected BaseObject _effectPrefab;

        public virtual void InitBossSkill(Tower target, float speed)
        {
            _targetTower = target;
            _targetTile = target.CurrentTile;

            // 부모(ParabolaBullet -> BaseBullet)의 초기화 호출 (발사음 재생 포함)
            // 보스 탄환은 데미지가 0인 경우가 많으므로 0f를 전달합니다.
            base.Init(target.transform, 0f, speed);
        }

        // 적중 시 실행 로직 (부모 BaseBullet의 OnHit에서 호출됨)
        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_targetTower != null && _targetTower.gameObject.activeSelf)
            {
                // 타겟 타워가 여전히 처음 조준했던 타일 위에 있을 때만 효과 적용
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