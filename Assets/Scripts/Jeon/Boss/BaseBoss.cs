using System.Collections;
using UnityEngine;
using Dev.cheol.Model;
namespace Dev.jeon.Model
{

    public abstract class BaseBoss : Enemy
    {
        [Header("Boss Skill Setting")]
        [SerializeField] protected float _skillInterval = 4f;       // 스킬 대기 시간
        [SerializeField] protected float _skillMotionDuration = 1.5f; // 스킬 모션 시간

        private float _skillTimer = 0f;
        protected bool _isUsingSkill  = false;

        public override void ObjectUpdate()
        {
            if (_isUsingSkill) return;

            base.ObjectUpdate();

            _skillTimer += Time.deltaTime;
            if(_skillTimer >= _skillInterval)
            {
                _skillTimer = 0f;
                StartCoroutine(SkillSequence());
            }
        }

        private IEnumerator SkillSequence()
        {
            _isUsingSkill = true;

            // 1. 스킬 모션 시작
            ChangeState(EState.IDLE);
            Debug.Log($"{this.name}이 스킬을 시전한다.!");
            // animator.SetTrigger("Skill");

            yield return new WaitForSeconds(_skillMotionDuration);

            ApplySkillEffect();

            _isUsingSkill = false;
            ChangeState(EState.MOVE);
        }
        protected abstract void ApplySkillEffect();
    }

}