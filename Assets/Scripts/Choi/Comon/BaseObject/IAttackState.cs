using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Dev.cheol.Comon
{
    public class IAttackState : IState
    {
        public IEnumerator Enter(BaseUnit unit)
        {
            unit.Animator.SetInteger("animation", unit.AttackAniNum);

            //unit.Animator.Play("ATK0", 0, 0f);
            unit.ActiveAttack();

            yield break;
        }

        public IEnumerator Execute(BaseUnit unit)
        {
            while (true)
            {
                // 타겟 유효성 검사
                if (unit.Target == null)
                {
                    unit.ChangeState(EState.IDLE);
                    yield break;
                }

                // 타겟 방향 회전 기존 로직 유지
                Vector3 direction = (unit.Target.position - unit.transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, targetRotation, Time.deltaTime * 10f);
                }

                // 애니메이션 종료 체크 단순화
                AnimatorStateInfo stateInfo = unit.Animator.GetCurrentAnimatorStateInfo(0);

                // 애니메이션이 끝나면 IDLE로 복귀
                if (stateInfo.normalizedTime >= 1.0f && !unit.Animator.IsInTransition(0))
                {
                    unit.ChangeState(EState.IDLE);
                    yield break;
                }

                yield return null;
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {
            yield break;
        }


    }

}

