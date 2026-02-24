using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dev.cheol.Stats
{

    [Serializable]
    public struct StatusInfo
    {
        public int MaxHp;
        public int Hp;
        /// <summary>
        /// 에너미는 이속, 타워는 공속임
        /// </summary>
        public float speed;


        //TODO : 데이터 나중에 넣어주세요 아무나

    }
}

