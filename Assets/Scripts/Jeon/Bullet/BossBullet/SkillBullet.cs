using UnityEngine;
using Dev.cheol.Model;
using Dev.cheol.Manager;

namespace Dev.jeon.Bullet
{
    // 보스가 던지는 스킬 구체 (포물선 이동을 위해 ParabolaBullet 상속)
    public class SkillBullet : ParabolaBullet
    {
        public enum ESkillType { ICE, SHAMAN }

        [Header("Skill Settings")]
        [SerializeField] private ESkillType _skillType;

        /// <summary>
        /// 타워를 타겟으로 스킬 탄 초기화
        /// </summary>
        public void InitSkill(Tower targetTower, float speed, ESkillType type)
        {
            _skillType = type;
            // 부모(ParabolaBullet)의 Init을 호출하여 포물선 이동 시작
            // 데미지는 0으로 설정
            base.Init(targetTower.transform, 0f, speed);
        }

        // 부모의 HitTarget을 오버라이드하여 데미지 대신 스킬 효과 적용
        protected override void HitTarget()
        {
            if (_target == null)
            {
                ReturnToPool();
                return;
            }

            // 타겟에서 Tower 컴포넌트 가져오기
            var tower = _target.GetComponent<Tower>();

            if (tower != null)
            {
                switch (_skillType)
                {
                    case ESkillType.ICE:
                        tower.Seal(); // 빙결: 타워 멈춤
                        break;
                    case ESkillType.SHAMAN:
                        tower.Downgrade(); // 주술사: 랭크 다운
                        break;
                }
            }

            ReturnToPool();
        }

        // ParabolaBullet에 ReturnToPool이 이미 있다면 생략 가능하지만, 
        // SkillBullet만의 특수한 반납 처리가 필요할 때만 남겨둡니다.
        private void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        public override void ObjectUpdate()
        {
        }

        protected override void OnDisable()
        {
            base.OnDisable();// 부모의 코루틴 정지 로직 실행
        }
    }
}