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
        IDLE = 1,
        MOVE = 2,
        ATTACK = 3,
        DEATH = 4,
        MAX = 9999
    }

    public abstract class BaseObject : MonoBehaviour
    {
        [SerializeField] protected string _poolTag;
        [SerializeField] protected Transform _target;
        [SerializeField] protected bool _isUI;

        public string PoolTag { get => _poolTag; set => _poolTag = value; }
        public Transform Target { get => _target; set => _target = value; }
        public bool IsUI { get => _isUI; set => _isUI = value; }

        // [추가] UI 여부를 확인하는 프로퍼티 (매니저에서 사용)


        protected virtual void Awake()
        {
            // RectTransform이 있으면 UI로 판단합니다.
            //IsUI = GetComponent<RectTransform>() != null;
        }

        public abstract void ObjectUpdate();


        //TODO : 라이프 사이클 정의 다되고 리팩토링 과정에 얘들 

        public virtual void OnSpawn() { }   // 풀에서 꺼낼 때 실행 (초기화용)
        public virtual void OnDespawn() { } // 풀에 들어갈 때 실행 (데이터 정리용)
    }
}