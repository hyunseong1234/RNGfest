using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Comon
{
    public class TargetFinder : MonoBehaviour
    {
        public Transform GetNearestTarget(BaseObject unit)
        {
            if (unit.Target != null) return unit.Target; //갈곳이 있다면 경로를 따로 잡아주지않음


            return GetNextPoint();
        }

        /// <summary>
        /// 기존 플레그 타입로직
        /// </summary>
        /// <returns></returns>
        public Transform GetNextPoint()
        {
            return transform;
        }
    }


}
