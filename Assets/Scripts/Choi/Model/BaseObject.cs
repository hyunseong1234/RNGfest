using Dev.cheol.Comon;
using Dev.cheol.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public enum EState
    {
        NONE = 0,
        IDLE = 1, //아이돌마스터
        MOVE = 2, //무브
        ATTACK = 3, //공격
        DEATH = 4, //죽음
        MAX = 9999
    }
    public abstract class BaseObject : MonoBehaviour
    {
        [SerializeField] protected string _poolTag;
        [SerializeField] protected Transform _target;

        public string PoolTag { get => _poolTag; set => _poolTag = value; }
        public Transform Target { get => _target; set => _target = value; }

        public abstract void ObjectUpdate();

    }
}