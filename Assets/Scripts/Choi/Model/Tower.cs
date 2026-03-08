using Dev.cheol.Manager;
using Dev.cheol.UI;
using UnityEngine;

namespace Dev.cheol.Model
{

    public class Tower : BaseUnit
    {
        [SerializeField] private GameObject _iceEffect; // 타워를 감싸는 얼음 모델링
        
        [SerializeField] private TileObject _currentTile;
        [SerializeField] private int _lank;
        [SerializeField] private LankStarUI _starUI;

        [SerializeField] private bool _isSealed = false; // 봉인 여부

        public EState _state;
        public bool IsSealed => _isSealed; //봉인 상태 여부
        public TileObject CurrentTile { get => _currentTile; set => _currentTile = value; }
        public int Lank { get => _lank; set => _lank = value; }
        public LankStarUI StarUI { get => _starUI; set => _starUI = value; }
        public EState CurrentState => _state;
        public override void ActiveAttack()
        {
            throw new System.NotImplementedException();
        }
        public override void ObjectUpdate()
        {
            if (_isSealed) return;

            base.ObjectUpdate(); // 버프 업데이트 실행
        }
        /// <summary>
        /// 랭크에 맞게 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rank"></param>
        public void Setup(TowerData data, int rank)
        {
            this.Lank = rank;

            // SO에서 해당 랭크의 스탯을 찾음
            var targetStat = data.stats.Find(s => s.rank == rank);

            if (targetStat != null && _stat != null)
            {
                // 핵심: _stat 내부의 Stat 객체들의 BaseValue에 직접 주입
                _stat.Damage.BaseValue = targetStat.attack;
                _stat.Speed.BaseValue = targetStat.speed;
                _stat.Range.BaseValue = targetStat.range;

                for (int i = 0; i < targetStat.specialValues.Count; i++)
                {
                    if (i < _stat.SpecialValues.Count)
                    {
                        _stat.SpecialValues[i].BaseValue = targetStat.specialValues[i];
                    }
                    else
                    {
                        // 만약 타워에 미리 정의된 Stat 객체가 부족하면 새로 생성하거나 경고
                        Debug.LogWarning($"{gameObject.name}: 타워에 정의된 SpecialStat 개수가 CSV 데이터보다 적습니다.");
                    }
                }
            }

        }
        public void Seal()
        {
            if (_isSealed) return;
            _isSealed = true;

            // 시각적 피드백 (예: 얼음 이펙트 활성화)
            ChangeState(EState.IDLE);

            // 시각적 피드백
            if (_animator != null)
            {
                _animator.speed = 0; // 애니메이션까지 완전히 멈추고 싶을 때
            }
            // 얼음 이펙트
            if (_iceEffect != null)
            {
                _iceEffect.SetActive(true);
            }
            Debug.Log($"{gameObject.name} 봉인됨!");
        }

        public void UnSeal()
        {
            _isSealed = false;
            // 시각적 피드백 해제
            if (_animator != null)
            {
                _animator.speed = 1; // 애니메이션 다시 재생
            }
            if (_iceEffect != null)
            {
                _iceEffect.SetActive(false);
            }
            Debug.Log($"{gameObject.name} 봉인 해제!");
        }

        public void Downgrade()
        {
            if (Lank > 1)
            {
                // 1. 랭크 감소
                Lank--;

                // 2. FactoryManager를 통해 내 데이터(TowerData)를 다시 가져와서 Setup 재실행
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                var data = factory.GetTowerData(PoolTag); // PoolTag 기반으로 데이터 탐색

                if (data != null)
                {
                    Setup(data, Lank); // 낮아진 랭크로 스탯 및 별 UI 갱신
                    Debug.Log($"{gameObject.name} 등급 하락! 현재 등급: {Lank}");
                }

                // 3. (선택 사항) 등급 하락 이펙트 재생
            }
            else
            {
                // 1성일 때 등급이 깎이면 파괴할지, 그대로 둘지 기획에 따라 결정
                Debug.Log($"{gameObject.name}은 이미 최하 등급(1성)입니다.");
            }
        }
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _isSealed = false; // 풀에 들어갈 때 봉인 해제

            if (_iceEffect != null) _iceEffect.SetActive(false);
        }

    }
}