using System.Collections;
using UnityEngine;
using Dev.cheol.Model;
namespace Dev.jeon.Model
{

    public abstract class BaseBoss : Enemy
    {
        [Header("Boss Skill Setting (Tile Based)")]
        [SerializeField] protected int _skillTileInterval = 4; // 4타일 이동 시마다 스킬 발동
        [SerializeField] protected float _skillMotionDuration = 1.5f;

        protected int _movedTileCount = 0;   // 이동한 타일 누적 카운트
        protected int _lastWaypointIndex = 0; // 직전 웨이포인트 인덱스
        protected bool _isUsingSkill = false;

        public override void ObjectUpdate()
        {
            if (_isUsingSkill) return;

            base.ObjectUpdate(); // Enemy의 기본 이동 로직

            // [핵심] 타일(웨이포인트)을 넘어갔는지 확인
            if (_waypointIndex > _lastWaypointIndex)
            {
                _movedTileCount++;
                _lastWaypointIndex = _waypointIndex;

                // 설정한 타일 수만큼 이동했다면 스킬 발동!
                if (_movedTileCount >= _skillTileInterval)
                {
                    _movedTileCount = 0;
                    StartCoroutine(SkillSequence());
                }
            }
        }

        private IEnumerator SkillSequence()
        {
            _isUsingSkill = true;

            // 1. 기 모으기 (멈춤)
            ChangeState(EState.IDLE);

            yield return new WaitForSeconds(_skillMotionDuration);

            // 2. 자식 클래스의 스킬 실행 
            yield return StartCoroutine(ApplySkillEffectRoutine());

            // 3. 다시 이동 재개
            _isUsingSkill = false;
            ChangeState(EState.MOVE);
        }

        protected abstract IEnumerator ApplySkillEffectRoutine();
    }

}