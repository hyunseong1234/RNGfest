using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class TowerManager : BaseManager
    {
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }



        /// <summary>
        /// 동일 태그 동일 랭크가 맞는지 확인 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool CanMerge(Tower origin, Tower target)
        {
            return origin.PoolTag == target.PoolTag && origin.Lank == target.Lank;
        }

        public int LankCalculator()
        {
            return 1;
        }

    }

}
