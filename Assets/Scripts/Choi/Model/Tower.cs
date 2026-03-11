using Dev.cheol.Manager;
using Dev.cheol.UI;
using System.Collections;
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
                    StarUI.Init(this);
                }
            }
            else
            {
                StartCoroutine(DestroyRoutine(0.5f));
            }
        }
        private IEnumerator DestroyRoutine(float delay)
        {
            // 1. 기능 즉시 정지
            _isSealed = true;
            if (_animator != null) _animator.speed = 0;
            if (StarUI != null) StarUI.gameObject.SetActive(false); // 별 숨기기

            // 2. 타일 비워주기 (그래야 플레이어가 바로 다른 타워 지음)
            if (CurrentTile != null) CurrentTile._isUsed = false;

            // 3. 이펙트 터지는 시간 대기
            yield return new WaitForSeconds(delay);

            // 4. 최종적으로 게임에서 제거 및 풀 반납
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager != null) mainManager.RemoveUnit(this);
        }
        public void DowngradEffect(BaseObject downgradePrefab, BaseObject destroyPrefab)
        {
            // 중요: 현재 랭크를 미리 저장해둡니다. (Downgrade() 호출 후에는 값이 바뀌기 때문)
            int beforeLank = Lank;

            // 1. 실제 랭크 감소 로직 실행
            Downgrade();

            // 2. 상황에 맞는 연출 실행
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (beforeLank > 1)
            {
                // [일반 강등 연출] 2성 -> 1성 등
                if (downgradePrefab != null)
                {
                    var effect = pool.GetFromPool<BaseObject>(downgradePrefab.name);
                    if (effect != null)
                    {
                        effect.gameObject.SetActive(true);
                        effect.transform.position = this.transform.position + Vector3.up * 0.5f;
                        Debug.Log($"<color=magenta>[Tower] {gameObject.name} 강등 연출!</color>");
                    }
                }
            }
            else
            {
                // [파괴 연출] 1성 -> 파괴
                if (destroyPrefab != null)
                {
                    var effect = pool.GetFromPool<BaseObject>(destroyPrefab.name);
                    if (effect != null)
                    {
                        effect.gameObject.SetActive(true);
                        // 파괴될 때는 타워 발밑이나 중앙에서 크게 터지는 느낌
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