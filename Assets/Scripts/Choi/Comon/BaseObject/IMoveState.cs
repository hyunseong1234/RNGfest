using Dev.cheol.Model;
using Dev.cheol.Stats;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;


namespace Dev.cheol.Comon
{
    public class IMoveState : IState
    {
        public IEnumerator Enter(BaseUnit unit)
        {
            Debug.Log("움직임 시작");
            yield break;
        }
        public IEnumerator Execute(BaseUnit unit)
        {
            if (unit._stat.Speed.Value <= 0) yield break;

            while (true)
            {
                // 타겟이 사라지면 루프 탈출 (Exit로 이동)
                if (unit.Target == null) yield break;

                // 실제 이동 처리
                unit.transform.position = Vector3.MoveTowards(
                    unit.transform.position,
                    unit.Target.position,
                    unit._stat.Speed.Value * Time.deltaTime
                );

                yield return null;
            }
        }

        public IEnumerator Exit(BaseUnit unit)
        {
            yield break;
        }

    }

}

