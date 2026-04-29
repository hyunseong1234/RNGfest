using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Model
{
    public abstract class BaseBoss : Enemy
    {
        [Header("Boss Skill Setting (Tile Based)")]
        [SerializeField] protected int _skillTileInterval = 4;
        [SerializeField] protected float _skillMotionDuration = 1.5f;

        protected int _movedTileCount = 0;
        protected int _lastWaypointIndex = 0;
        protected bool _isUsingSkill = false;

        public override void ObjectUpdate()
        {
            if (_isUsingSkill) return;
            base.ObjectUpdate();

            if (_waypointIndex > _lastWaypointIndex)
            {
                _movedTileCount++;
                _lastWaypointIndex = _waypointIndex;

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
            ChangeState(EState.IDLE);
            yield return new WaitForSeconds(_skillMotionDuration);
            yield return StartCoroutine(ApplySkillEffectRoutine());
            _isUsingSkill = false;
            ChangeState(EState.MOVE);
        }

        protected abstract IEnumerator ApplySkillEffectRoutine();

        /// <summary>
        /// 보스 처치 시 → AugmentManager에 알림
        /// Enemy.cs의 OnDamaged()에서 hp <= 0 되면 OnDeath() 호출됨
        /// </summary>
        protected override void OnDeath()
        {
            base.OnDeath();

            var augmentManager = ServiceLocator.Instance.GetService<AugmentManager>();
            if (augmentManager != null)
            {
                augmentManager.OnBossDefeated();
            }
            else
            {
                Debug.LogWarning("[BaseBoss] AugmentManager를 찾을 수 없습니다. ServiceLocator에 등록됐는지 확인하세요.");
            }
        }
    }
}