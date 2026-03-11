using Dev.cheol.Manager;
using Dev.cheol.UI;
using System.Collections;
using UnityEngine;

namespace Dev.cheol.Model
{

    public class Tower : BaseUnit
    {
        [SerializeField] private BaseObject _currentEffect; // 타워를 감싸는 얼음 모델링
        [SerializeField] private float _destroyTime = 1.0f; // 0성 되면 파괴 되는 시간
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
        public void Seal(BaseObject effectPrefab)
        {
            // 1. 함수가 아예 호출조차 안 되는지 확인하는 로그
            Debug.Log($"<color=cyan>1. [Tower] {gameObject.name}의 Seal 함수가 호출됨!</color>");

            if (_isSealed)
            {
                Debug.Log("<color=red>-> 이미 봉인된 상태라 취소됨!</color>");
                return;
            }
            _isSealed = true;

            if (effectPrefab != null)
            {
                Debug.Log($"<color=cyan>2. 전달받은 프리팹 이름: {effectPrefab.name}</color>");
                var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

                var effect = pool.GetFromPool<BaseObject>(effectPrefab);

                if (effect != null)
                {
                    Debug.Log("<color=green>3. 풀링 매니저가 얼음을 정상적으로 줬습니다!</color>");
                    effect.transform.SetParent(this.transform, false);
                    effect.transform.localPosition = Vector3.zero;

                    // [추가 안전장치] 풀에서 나왔는데 크기가 0이거나 꺼져있을 확률 100% 차단
                    effect.transform.localScale = Vector3.one;
                    effect.gameObject.SetActive(true);

                    _currentEffect = effect;
                }
                else
                {
                    // 여기가 뜨면 풀링 매니저 설정 문제입니다!
                    Debug.LogError("<color=red>3. [에러] 풀링 매니저가 얼음을 주지 않고 null을 반환했습니다! (풀에 등록 안 됨)</color>");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>{gameObject.name}: 인스펙터에 프리팹이 안 들어있습니다!</color>");
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

        public void Downgrade(float delay)
        {
            if (Lank > 1)
            {
                Lank--;
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                var data = factory.GetTowerData(PoolTag);
                if (data != null) Setup(data, Lank);
                if (StarUI != null) StarUI.Init(this);
            }
            else
            {
                // 총알이 보내준 delay 시간을 사용합니다.
                StartCoroutine(DestroyRoutine(delay));
            }
        }
        private IEnumerator DestroyRoutine(float delay)
        {
            // 1. 기능 정지 (타워가 더 이상 공격 못 하게 봉인)
            _isSealed = true;
            if (_animator != null) _animator.speed = 0;
            if (StarUI != null) StarUI.gameObject.SetActive(false); // 별 UI 끄기

            // 2. 타일 비워주기 (그래야 플레이어가 즉시 다음 타워를 건설함)
            if (CurrentTile != null)
            {
                CurrentTile._isUsed = false;
            }

            // 3. 총알에서 넘겨준 시간(delay)만큼 대기 (연출 감상 타임)
            yield return new WaitForSeconds(delay);

            // 4. 최종적으로 매니저에서 삭제하고 풀(Pool)로 반납
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager != null)
            {
                mainManager.RemoveUnit(this);
            }
        }
        // 2. DowngradEffect 함수도 시간을 받아서 Downgrade에 전달
        public void DowngradEffect(BaseObject downgradePrefab, BaseObject destroyPrefab, float delay)
        {
            int beforeLank = Lank;

            // 로직 실행 시 총알이 준 시간을 전달
            Downgrade(delay);

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            // 3. 1성인지 아닌지에 따라 "딱 하나"의 이펙트만 생성
            if (beforeLank > 1)
            {
                if (downgradePrefab != null)
                {
                    var effect = pool.GetFromPool<BaseObject>(downgradePrefab);
                    if (effect != null)
                    {
                        effect.gameObject.SetActive(true);
                        effect.transform.position = this.transform.position + Vector3.up * 0.5f;
                    }
                }
            }
            else
            {
                if (destroyPrefab != null)
                {
                    var effect = pool.GetFromPool<BaseObject>(destroyPrefab);
                    if (effect != null)
                    {
                        effect.gameObject.SetActive(true);
                        effect.transform.position = this.transform.position;
                    }
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