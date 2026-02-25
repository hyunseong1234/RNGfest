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
        private bool _hasFired = false; // 한 번의 공격 애니메이션에서 발사 여부 체크
        public IEnumerator Enter(BaseUnit unit)
        {
            Debug.Log("공격 호출");
            unit.Animator.SetInteger("animation", 2);

            yield break;
        }

        public IEnumerator Execute(BaseUnit unit)
        {
            // 공격을 시작할 때 초기화
            _hasFired = false;

            while (true) // 상태가 바뀌어서 StopCoroutine이 호출될 때까지 무한 반복
            {
                if (unit.Target == null) yield break;

                Vector3 direction = (unit.Target.position - unit.transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    float rotSpeed = unit.Status.Speed * 10f; //회전속도 나중에 
                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
                }

                AnimatorStateInfo stateInfo = unit.Animator.GetCurrentAnimatorStateInfo(0);

                if (!_hasFired && stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 0.5f)
                {
                    _hasFired = true;
                    FireProjectile(unit);
                }

                // 애니메이션 종료 체크
                if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f && !unit.Animator.IsInTransition(0))
                {
                    unit.Animator.SetInteger("animation", 0);
                    unit.ChangeState(EState.IDLE);
                    yield break; // 상태가 바뀌었으니 루프 탈출
                }

                yield return null; // 다음 프레임까지 대기
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {

            yield break;
        }

        private void FireProjectile(BaseObject unit)
        {
            // 여기서 실제로 총알 생성 및 발사 로직 수행
            Debug.Log($"{unit.name} 발사!");
            // 예: ObjectPoolingManager.Instance.GetBullet(unit.Target);
        }
    }

}

