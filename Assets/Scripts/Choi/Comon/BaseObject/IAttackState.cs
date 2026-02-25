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
        private bool _hasFired = false; // 한 번의 공격 애니메이션에서 발사 여부 체크
        public IEnumerator Enter(BaseUnit unit)
        {
            unit.Animator.SetInteger("animation", 2);

            unit.Animator.Play("ATK0", 0, 0f);

            // 3. 발사 플래그 초기화
            _hasFired = false;
            yield break;
        }

        public IEnumerator Execute(BaseUnit unit)
        {
            // 공격 상태 진입 시 발사 여부 초기화
            _hasFired = false;
            string ATK0 = "ATK0";


            while (true)
            {
                // 1. 타겟 유효성 검사
                if (unit.Target == null)
                {
                    Debug.Log($"{unit.name} : 타겟이 없어 공격 상태를 종료합니다.");
                    unit.ChangeState(EState.IDLE);
                    yield break;
                }

                // 2. 타겟 방향으로 회전
                Vector3 direction = (unit.Target.position - unit.transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    float rotSpeed = 10f; // 필요시 unit.Status.RotationSpeed 등으로 교체
                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
                }

                // 3. 애니메이터 정보 가져오기 (0번 레이어)
                AnimatorStateInfo stateInfo = unit.Animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsName("ATK1"))
                {
                    Debug.Log("애니메이션 조건 맞음");
                }

                if (stateInfo.IsName(ATK0))
                {
                    if (!_hasFired)
                    {
                        if (stateInfo.normalizedTime >= 0.5f)
                        {
                            unit.ActiveAttack();
                            _hasFired = true;
                            Debug.Log($"<color=cyan>{unit.name} 탄 발사 호출!</color>");
                        }
                    }

                    if (stateInfo.normalizedTime >= 1.0f && !unit.Animator.IsInTransition(0))
                    {
                        Debug.Log($"{unit.name} 공격 애니메이션 완료, IDLE로 전환");
                        unit.Animator.SetInteger("animation", 0);
                        unit.ChangeState(EState.IDLE);
                        yield break;
                    }


                }

                yield return null; // 다음 프레임까지 대기
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {
            Debug.Log("T시팔 호출됫나??");
            yield break;
        }


    }

}

