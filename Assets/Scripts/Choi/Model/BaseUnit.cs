using Dev.cheol.Comon;
using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{


    public abstract class BaseUnit : BaseObject
    {
        [Header("기본 필드")]
        [SerializeField] protected StatusInfo _status;


        [Header("상태관련")]
        public Coroutine currentStateCoroutine;
        [SerializeField] private EState currentState = EState.IDLE;
        protected Dictionary<EState, IState> stateDictionary = new();
        public Dictionary<EState, int> IndexPair = new();

        [Header("아니메")]
        [SerializeField] protected Animator _animator = null;
        //[SerializeField] protected AnimData _animData;
        //[SerializeField] private AnimConfig _animConfig;
        //private Dictionary<AniModel, int> _animTable = new Dictionary<AniModel, int>();


        #region  프로퍼티
        public StatusInfo Status { get => _status; set => _status = value; }
        public Animator Animator { get => _animator; set => _animator = value; }

        #endregion


        protected virtual void Awake()
        {
            //상태 머신 등록
            stateDictionary.Add(EState.IDLE, new IIdleState());
            stateDictionary.Add(EState.MOVE, new IMoveState());
            stateDictionary.Add(EState.ATTACK, new IAttackState());

            IndexPair.Add(EState.IDLE, 0);
            IndexPair.Add(EState.MOVE, 1);
            IndexPair.Add(EState.ATTACK, 2);

            //foreach (var data in _animConfig.datas)
            //{
            //    // Enum.ToString()을 쓰기로 했다면 여기서 해시 생성
            //    int hash = Animator.StringToHash(data.eAniName.ToString());
            //    _animTable[data.eAniName] = hash;
            //}

            if (_animator != null) return;
            _animator = GetComponentInChildren<Animator>();
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


        /// <summary>
        /// 객체가 사라질때 초기화 코드
        /// </summary>
        public virtual void OnReturnToPool()
        {

            StopAllCoroutines();
            currentStateCoroutine = null;

            Target = null;

            ChangeState(EState.IDLE);
        }

        private IEnumerator RunStateLifeCyle(IState state)
        {
            yield return state.Enter(this);
            yield return state.Execute(this);
            yield return state.Exit(this);
        }

        public override void ObjectUpdate()
        {

        }
        public abstract void ActiveAttack();
    }
}
