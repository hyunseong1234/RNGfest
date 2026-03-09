using UnityEngine;
using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    public class SkillBullet : ParabolaBullet
    {
        public enum ESkillType { ICE, SHAMAN }
        [SerializeField] private ESkillType _skillType;

        private Tower _targetTower;
        private TileObject _targetTile;

        public void InitSkill(Tower target, float speed, ESkillType type)
        {
            _skillType = type;
            _targetTower = target;
            _targetTile = target.CurrentTile;

            // 부모의 Init 호출 (타겟 트랜스폼 넘김)
            base.Init(target.transform, 0f, speed);
        }

        // 부모의 로직을 완전히 덮어씌워서 타일 기반 판정을 수행
        protected override void HitTarget()
        {
            // 타겟 타워가 아직 그 타일 위에 그대로 있다면 효과 적용
            if (_targetTower != null && _targetTower.gameObject.activeSelf && _targetTower.CurrentTile == _targetTile)
            {
                if (_skillType == ESkillType.ICE) _targetTower.Seal();
                else _targetTower.Downgrade();
            }
            else
            {
                Debug.Log("<color=cyan>[Skill] 타워가 사라져서 바닥 충돌!</color>");
            }

            // 부모의 ReturnToPool 호출 (에러 없이 정상 작동)
            ReturnToPool();
        }

        // 부모의 함수를 오버라이드하여 안전하게 반납
        protected override void ReturnToPool()
        {
            base.ReturnToPool(); // 부모가 가진 SetActive(false)와 ReturnPool 실행
        }
    }
}