using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Dev.cheol.Stats
{
    [System.Serializable]
    public class BaseUnitStats
    {
        public Stat Speed; // 에너미 : 이동속도  / 타워 : 공격속도
        public Stat MaxHp;//hp
        public Stat Defense; // 방어력
        public float CurrentHp; //현재 체력
        public Stat Damage; //데미지
        public Stat Range; //
        public List<Stat> SpecialValues = new List<Stat>();
    }
}