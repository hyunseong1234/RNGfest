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
        [SerializeField] private string _poolTag;
        private Transform _target;
        [SerializeField] protected StatusInfo _status;

        public Coroutine currentStateCoroutine;
        [SerializeField] private EState currentState = EState.IDLE;

        // 상태 이넘과 실제 상태 객체(IState)를 매핑
        protected Dictionary<EState, IState> stateDictionary = new();
        public Dictionary<EState, int> IndexPair = new();

        public string PoolTag { get => _poolTag; set => _poolTag = value; }
        public Transform Target { get => _target; set => _target = value; }
        public StatusInfo Status { get => _status; set => _status = value; }

        public abstract void ObjectUpdate();

        private void Awake()
        {
            stateDictionary.Add(EState.IDLE, new IdleState());
            stateDictionary.Add(EState.MOVE, new MoveState());

            IndexPair.Add(EState.IDLE, 0);
            IndexPair.Add(EState.MOVE, 1);
        }
        // 이넘을 인자로 받는 ChangeState
        public void ChangeState(EState newStateEnum)
        {

            if (currentState == newStateEnum && currentStateCoroutine != null) return;

            if (stateDictionary.TryGetValue(newStateEnum, out IState nextState))
            {
                if (currentStateCoroutine != null)
                {
                    StopCoroutine(currentStateCoroutine);
                }

                currentState = newStateEnum;
                currentStateCoroutine = StartCoroutine(RunStateLifeCyle(nextState));
            }
            else
            {
                Debug.LogWarning($"{newStateEnum} 상태가 stateDictionary에 등록되지 않았습니다.");
            }
        }

        private IEnumerator RunStateLifeCyle(IState state)
        {
            yield return state.Enter(this);
            yield return state.Execute(this);
            yield return state.Exit(this);
        }
    }
}