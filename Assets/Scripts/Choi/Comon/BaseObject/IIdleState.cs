using Dev.cheol.Comon;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Comon
{
    public class IIdleState : IState
    {
        public IEnumerator Enter(BaseUnit unit)
        {
            Debug.Log("아이들시작");
            if (unit is Tower) unit.Animator.SetInteger("animation", 1); //임시로 사용                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, targetRotation, Time.deltaTime * unit.Status.Speed);


            yield break;
        }
        public IEnumerator Execute(BaseUnit unit)
        {
            // 정면(Vector3.forward) 또는 초기 회전값(Quaternion.identity)을 목표로 설정
            Quaternion targetRotation = Quaternion.Euler(0, 180, 0);

            // 현재 회전과 목표 회전의 각도 차이가 0.1도보다 크면 계속 회전
            while (Quaternion.Angle(unit.transform.rotation, targetRotation) > 0.1f)
            {
                unit.transform.rotation = Quaternion.Slerp(
                    unit.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f  // 회전값에 대한 정보가 없음 그래서 매직 넘버 
                );

                yield return null; // 다음 프레임까지 대기
            }

            // 각도가 거의 일치하면 완전히 고정
            unit.transform.rotation = targetRotation;

            // 회전이 끝난 후에도 상태가 바뀌기 전까지는 코루틴이 종료되지 않게 대기
            while (true)
            {
                yield return null;
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {
            yield break;
        }

    }

}
