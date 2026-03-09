using Dev.cheol.Comon;
using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
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
        public BaseUnitStats _stat;
        [Header("버프 관리")]
        protected List<BaseBuff> _buffs = new List<BaseBuff>();

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

            if (_animator != null) return;
            _animator = GetComponentInChildren<Animator>();
        }
        // 이넘을 인자로 받는 ChangeState
        public void ChangeState(EState newStateEnum)
        {
            // [추가] 오브젝트가 꺼져있으면 코루틴을 돌릴 수 없으므로 리턴
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
        //  외부에서 버프를 추가하는 함수
        public void AddBuff(BaseBuff newBuff)
        {
            // 새 버프 초기화 및 리스트 추가
            // (이미 Init이 된 상태로 들어올 수도 있으니 상황에 맞게 조정)
            if (newBuff != null)
            {
                _buffs.Add(newBuff);
            }
        }

        //  버프 전체 해제 (풀 반납 시 호출)
        public void ClearAllBuffs()
        {
            foreach (var buff in _buffs)
            {
                buff.EndBuff(); // 스탯 복구 등 마무리 작업
            }
            _buffs.Clear();
        }
        // 기존 버프 찾는 기능 추가
        public T GetBuff<T>() where T : BaseBuff
        {
            // 내 몸에 붙은 버프들을 쭉 뒤져서
            for (int i = 0; i < _buffs.Count; i++)
            {
                // 찾고 있는 타입(예: PoisonBuff)이 있으면 그걸 반환
                if (_buffs[i] is T typedBuff)
                {
                    return typedBuff;
                }
            }
            return null; // 없으면 null
        }


        public override void ObjectUpdate()
        {
            if (_buffs == null || _buffs.Count == 0) return;

            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                if (i >= _buffs.Count) continue;

                var targetBuff = _buffs[i];
                if (targetBuff == null)
                {
                    _buffs.RemoveAt(i);
                    continue;
                }

                // 1. 업데이트 실행
                targetBuff.BuffUpdate(Time.deltaTime);

                // [안전장치 2] BuffUpdate 실행 도중 리스트가 변했을 가능성 체크
                if (i >= _buffs.Count || _buffs[i] != targetBuff) continue;

                // 2. 수명 다한 버프 리스트에서 제거
                if (targetBuff.IsFinished)
                {
                    targetBuff.EndBuff(); // 끝날 때 처리 명시적 호출 (필요시)
                    _buffs.RemoveAt(i);
                }
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

            // 풀로 돌아갈 때 버프 싹 지우기
            ClearAllBuffs();
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
