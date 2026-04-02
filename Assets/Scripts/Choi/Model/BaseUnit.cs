using Dev.cheol.Comon;
using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using Dev.jeon.Buff;
using Dev.jeon.Bullet;
using Dev.jeon.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{


    public abstract class BaseUnit : BaseObject
    {
        protected BuffHandler _buffHandler;
        public BaseUnitStats _stat;

        [Header("상태관련")]
        public Coroutine currentStateCoroutine;
        [SerializeField] private EState currentState = EState.IDLE;
        protected Dictionary<EState, IState> stateDictionary = new();
        public Dictionary<EState, int> IndexPair = new();

        [Header("아니메")]
        [SerializeField] protected Animator _animator = null;
        [SerializeField] protected int _attackAniNum = 2; //각자 들고있는 번호가 다르기때문에 인트 넘버
        //[SerializeField] protected AnimData _animData;
        //[SerializeField] private AnimConfig _animConfig;
        //private Dictionary<AniModel, int> _animTable = new Dictionary<AniModel, int>();


        #region  프로퍼티

        public Animator Animator { get => _animator; set => _animator = value; }
        public int AttackAniNum { get => _attackAniNum; set => _attackAniNum = value; }

        #endregion


        protected override void Awake()
        {
            base.Awake();
            //상태 머신 등록
            stateDictionary.Add(EState.IDLE, new IIdleState());
            stateDictionary.Add(EState.MOVE, new IMoveState());
            stateDictionary.Add(EState.ATTACK, new IAttackState());

            IndexPair.Add(EState.IDLE, 0);
            IndexPair.Add(EState.MOVE, 1);
            IndexPair.Add(EState.ATTACK, 2);

            //만약에 0 값으로 초기화 됬을때를 대비하여 
            if (_attackAniNum == 0) _attackAniNum = 2;

            //foreach (var data in _animConfig.datas)
            //{
            //    // Enum.ToString()을 쓰기로 했다면 여기서 해시 생성
            //    int hash = Animator.StringToHash(data.eAniName.ToString());
            //    _animTable[data.eAniName] = hash;
            //}
            _buffHandler = new BuffHandler(this);

            if (_animator != null) return;
            _animator = GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// 상태 머신 교체하는 로직 분기점 비교
        /// </summary>
        /// <param name="newStateEnum"></param>
        public void ChangeState(EState newStateEnum)
        {
            if (!gameObject.activeInHierarchy) return;
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


        //TODO : 나중에 버퍼 핸들러 클래스 추가하하고 거기로 기능 옮겨 놔야됨 핸들러는 가급적 모노비헤이비어 상속 받지 말것

        #region 버퍼 핸들러
        //  외부에서 버프를 추가하는 함수
        public void AddBuff(BaseBuff newBuff, float duration, BaseObject effectPrefab)
        {
            _buffHandler.AddBuff(newBuff, duration, effectPrefab);
        }

        public T GetBuff<T>() where T : BaseBuff
        {
            return _buffHandler.GetBuff<T>();
        }

       
        #endregion

        public override void ObjectUpdate()
        {
            _buffHandler.HandleUpdate(Time.deltaTime);
        }


        /// <summary>
        /// 객체가 사라질때 초기화 코드
        /// </summary>
        public virtual void OnReturnToPool()
        {

            StopAllCoroutines();
            _buffHandler.ClearAll(); 
            ChangeState(EState.IDLE);
        }


        private IEnumerator RunStateLifeCyle(IState state)
        {
            yield return state.Enter(this);
            yield return state.Execute(this);
            yield return state.Exit(this);
        }


        public abstract void ActiveAttack();
    }
}
