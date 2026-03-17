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

            if (unit.Animator != null)
            {
                unit.Animator.speed = unit._stat.Speed.Value;
            }

            unit.Animator.SetInteger("animation", unit.AttackAniNum);
            unit.ActiveAttack();

            yield break;
        }

        public IEnumerator Execute(BaseUnit unit)
        {
            while (true)
            {
                // 1. 타겟 유효성 검사
                if (unit.Target == null || !unit.Target.gameObject.activeSelf)
                {
                    unit.ChangeState(EState.IDLE);
                    yield break;
                }

                // 2. 타겟 방향으로 회전 로직
                Vector3 direction = (unit.Target.position - unit.transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, targetRotation, Time.deltaTime * 10f);
                }

                // 애니메이션 종료 체크
                AnimatorStateInfo stateInfo = unit.Animator.GetCurrentAnimatorStateInfo(0);


                //애니메이션 끝나는 로직
                if (stateInfo.normalizedTime >= 1.0f && !unit.Animator.IsInTransition(0))
                {
                    unit.Animator.speed = 1.0f;
                    unit.ChangeState(EState.IDLE);

                    yield break;
                }

                yield return null;
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {
            if (unit.Animator != null)
            {
                unit.Animator.speed = 1.0f;
            }

            yield break;
        }


    }

}

