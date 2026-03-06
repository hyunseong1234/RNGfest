using Dev.cheol.Manager;
using Dev.cheol.UI;
using UnityEngine;

namespace Dev.cheol.Model
{

    public class Tower : BaseUnit
    {
        [SerializeField] private TileObject _currentTile;
        [SerializeField] private int _lank;
        [SerializeField] private LankStarUI _starUI;

        [SerializeField] private bool _isSealed = false; // 봉인 여부
        public bool IsSealed => _isSealed; //봉인 상태 여부
        public TileObject CurrentTile { get => _currentTile; set => _currentTile = value; }
        public int Lank { get => _lank; set => _lank = value; }
        public LankStarUI StarUI { get => _starUI; set => _starUI = value; }

        public override void ActiveAttack()
        {
            throw new System.NotImplementedException();
        }
        public override void ObjectUpdate()
        {
            base.ObjectUpdate(); // 버프 업데이트 실행

            // [추가] 봉인 상태라면 공격 로직을 실행하지 않음
            if (_isSealed)
            {
                ChangeState(EState.IDLE);
                return;
            }
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
            _isSealed = true;
            // 시각적 피드백 (예: 얼음 이펙트 활성화)
            Debug.Log($"{gameObject.name} 봉인됨!");
        }

        public void UnSeal()
        {
            _isSealed = false;
            // 시각적 피드백 해제
            Debug.Log($"{gameObject.name} 봉인 해제!");
        }

        public void Downgrade()
        {
            if (Lank > 1)
            {
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                var data = factory.GetTowerData(PoolTag); // 자신의 데이터를 가져옴
                if (data != null)
                {
                    Setup(data, Lank - 1);
                }
            }
        }
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _isSealed = false; // 풀에 들어갈 때 봉인 해제
        }

    }
}