using Dev.cheol.Manager;
using Dev.cheol.Stats;
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

        // 모디파이어를 변수로 들고 있어야 나중에 '교체'가 가능함
        private StatModifier _dmgMod;
        private StatModifier _atkSpeedMod;

        public EState _state;
        public bool IsSealed => _isSealed; //봉인 상태 여부
        public TileObject CurrentTile { get => _currentTile; set => _currentTile = value; }
        public int Lank { get => _lank; set => _lank = value; }
        public LankStarUI StarUI { get => _starUI; set => _starUI = value; }
        public EState CurrentState => _state;

        private BaseObject _hitEffect;

        private void Start()
        {
            var sys = ServiceLocator.Instance.GetService<SystemManager>();

            // 1. 업그레이드 신호 구독
            sys.OnUpgradeChanged += ApplyUpgrade;

            // 2. 소환 시점에 이미 올라가 있는 업그레이드 적용
            for (int i = 0; i < sys.Upgrades.Length; i++)
            {
                ApplyUpgrade(i, sys.Upgrades[i]);
            }
        }

        private void ApplyUpgrade(int index, int level)
        {
            if (level <= 0) return;

            switch (index)
            {
                case 0: // 공격력 업그레이드 (고정치 Flat 증가 예시)
                    if (_dmgMod != null) _stat.Damage.RemoveModifier(_dmgMod);
                    _dmgMod = new StatModifier(level * 5.0f, StatModType.Flat, this);
                    _stat.Damage.AddModifier(_dmgMod);
                    break;

                case 1: // 공격속도 업그레이드 (퍼cent 증가 예시)
                    if (_atkSpeedMod != null) _stat.Speed.RemoveModifier(_atkSpeedMod);
                    // 레벨당 5%씩 빨라짐
                    _atkSpeedMod = new StatModifier(level * 0.05f, StatModType.Percent, this);
                    _stat.Speed.AddModifier(_atkSpeedMod);
                    break;
            }
        }
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

        public void ApplyHitEffect(BaseObject hitPrefab)
        {
            if (hitPrefab == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var effect = pool.GetFromPool<BaseObject>(hitPrefab);

            if (effect != null)
            {
                // 타워의 자식으로 만들어서 따라다니게 함
                effect.transform.SetParent(this.transform, false);
                effect.transform.localPosition = new Vector3(0, 0.5f, 0); // 위치 보정

                effect.gameObject.SetActive(true);

                // 이전 히트 이펙트가 있다면 풀로 반납 (혹시 여러 번 맞을 경우 대비)
                if (_hitEffect != null)
                {
                    _hitEffect.gameObject.SetActive(false);
                    _hitEffect.transform.SetParent(null);
                    pool.ReturnPool(_hitEffect);
                }

                // 내가 보관함!
                _hitEffect = effect;
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
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (_currentEffect != null)
            {
                _currentEffect.gameObject.SetActive(false);
                _currentEffect.transform.SetParent(null);
                pool.ReturnPool(_currentEffect);
                _currentEffect = null;
            }
            if (_hitEffect != null)
            {
                _hitEffect.transform.SetParent(null);
                _hitEffect.gameObject.SetActive(false);
                pool.ReturnPool(_hitEffect);
                _hitEffect = null;
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
                        pool.StartCoroutine(ReturnEffectToPool(pool, effect, 1.5f));
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
                        pool.StartCoroutine(ReturnEffectToPool(pool, effect, 1.5f));
                    }
                }
            }
        }
        //이펙트 수거 전용 코루틴
        private IEnumerator ReturnEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (effect != null && effect.gameObject.activeSelf)
            {
                // 1. 다음번에 튀어나올 때 위치가 꼬이지 않게 부모 해제
                effect.transform.SetParent(null);

                // 2. 확실하게 화면에서 끄기 (이게 있어야 풀 매니저가 '비어있음'으로 인식함)
                effect.gameObject.SetActive(false);

                // 3. 풀로 반납
                pool.ReturnPool(effect);
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
        public override void OnReturnToPool()
        {
            UnSeal();

            base.OnReturnToPool();

        }
        private void OnDisable()
        {
            var sys = ServiceLocator.Instance.GetService<SystemManager>();
            if (sys != null)
            {
                sys.OnUpgradeChanged -= ApplyUpgrade;
            }
        }

    }
}