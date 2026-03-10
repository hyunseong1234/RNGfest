using Dev.cheol.Manager;
using Dev.cheol.UI;
using UnityEngine;

namespace Dev.cheol.Model
{

    public class Tower : BaseUnit
    {
        [SerializeField] private BaseObject _currentEffect; // 타워를 감싸는 얼음 모델링

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
        public void Seal(GameObject effectPrefab) 
        {
            if (_isSealed) return;
            _isSealed = true;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            var effect = pool.GetFromPool<BaseObject>(effectPrefab.GetComponent<BaseObject>());

            if (effect != null)
            {
                effect.transform.SetParent(this.transform, false);
                effect.transform.localPosition = Vector3.zero;

                _currentEffect = effect; // 생성된 객체를 클래스 변수에 저장
            }

            ChangeState(EState.IDLE);
            if (_animator != null) _animator.speed = 0;
        }

        public void UnSeal()
        {
            _isSealed = false;

            if (_currentEffect != null)
            {
                var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

                pool.ReturnPool(_currentEffect);
                _currentEffect = null;
            }

            if (_animator != null) _animator.speed = 1;
        }

        public void Downgrade()
        {
            if (Lank > 1)
            {
                Lank--; // 등급 하락

                // 1. 스탯 갱신 (데이터 재설정)
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                var data = factory.GetTowerData(PoolTag);
                if (data != null) Setup(data, Lank);

                // 2. [핵심] 별 UI 갱신!!
                if (StarUI != null)
                {
                    StarUI.Init(this); // 다시 Init을 호출하면 깎인 Lank에 맞춰 별이 바뀜
                }
            }
            else
            {
                // 1성일 때 다운그레이드 시 파괴 로직 (이전 답변 참고)
                Debug.Log($"{gameObject.name}이 1성에서 강등되어 파괴됩니다!");

                // 1. 타일 점유 해제 (유저님이 만든 _isUsed 활용)
                if (CurrentTile != null)
                {
                    CurrentTile._isUsed = false;
                }

                // 2. 메인 매니저를 통해 리스트에서 지우고 풀로 반납
                var mainManager = ServiceLocator.Instance.GetService<MainManager>();
                if (mainManager != null)
                {
                    mainManager.RemoveUnit(this);
                }
            }
        }
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _isSealed = false; // 풀에 들어갈 때 봉인 해제

            if (_currentEffect != null)
            {
                var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
                if (pool != null)
                {
                    pool.ReturnPool(_currentEffect);
                }
                _currentEffect = null; // 참조 해제
            }

            // 3. 애니메이션 속도 복구 (혹시 멈춘 채로 들어갔을 경우 대비)
            if (_animator != null)
            {
                _animator.speed = 1;
            }
        }

    }
}