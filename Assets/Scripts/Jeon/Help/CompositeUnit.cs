using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

namespace Dev.Help
{
    public class CompositeUnit
    {
        //키버튼 입력됬을 때 기준으로 합병되는 기준
        public void Composite(Tower tower)
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var target = GetClosestCompositeTarget(tower, main.SpawnTowers, 50f);

            // 2. 찾았으면 합성 실행 (예외 처리는 함수 내부에서 끝냄)
            if (target != null)
            {
                PerformSynthesis(tower, target);
            }
        }


        private Tower GetClosestCompositeTarget(Tower origin, List<Tower> towers, float maxRange)
        {
            if (origin == null || towers == null) return null;

            Tower closest = null;
            float minSqrDistance = maxRange * maxRange; // 최대 사거리로 초기값 설정

            foreach (var target in towers)
            {
                if (target == null || target == origin) continue;

                // 유클리드 제곱 거리 계산
                float sqrDistance = (target.transform.position - origin.transform.position).sqrMagnitude;

                // 현재까지 찾은 거리보다 더 가깝다면 갱신
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closest = target;
                }
            }

            return closest; // 아무도 없으면 null, 있으면 가장 가까운 놈 리턴
        }

        private void PerformSynthesis(Tower origin, Tower material)
        {
            // 실제 합성(삭제 및 새 유닛 생성) 로직이 들어갈 곳
            Debug.Log($"{origin.name}와 {material.name}을 합성합니다!");


            //material.

        }
    }
}

