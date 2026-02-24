using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dev.cheol.Comon
{
    public class MoveState : IState
    {
        public IEnumerator Enter(BaseObject unit)
        {
            Debug.Log("움직임 시작");
            yield break;
        }
        public IEnumerator Execute(BaseObject unit)
        {
            if (unit.Status.speed <= 0) yield break;

            while (true)
            {
                // 타겟이 사라지면 루프 탈출 (Exit로 이동)
                if (unit.Target == null) yield break;

                // 실제 이동 처리
                unit.transform.position = Vector3.MoveTowards(
                    unit.transform.position,
                    unit.Target.position,
                    unit.Status.speed * Time.deltaTime
                );

                yield return null;
            }
        }

        public IEnumerator Exit(BaseObject unit)
        {
            yield break;
        }

    }

}

