using Dev.cheol.Manager;
using Dev.cheol.Stats;
using Dev.cheol.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public class Tower : BaseUnit
    {
        [Header("--- [덱 슬롯 설정] ---")]
        [SerializeField] private int _mySlotIndex;

        [Header("--- [기본 변수] ---")]
        [SerializeField] private BaseObject _currentEffect;
        [SerializeField] private float _destroyTime = 1.0f;
        [SerializeField] private TileObject _currentTile;
        [SerializeField] private int _lank;
        [SerializeField] private LankStarUI _starUI;
        [SerializeField] private bool _isSealed = false;

        private StatModifier _upgradeMod;

        public EState _state;
        public bool IsSealed => _isSealed;
        public TileObject CurrentTile { get => _currentTile; set => _currentTile = value; }
        public int Lank { get => _lank; set => _lank = value; }
        public int MySlotIndex { get => _mySlotIndex; set => _mySlotIndex = value; }
        public LankStarUI StarUI { get => _starUI; set => _starUI = value; }
        public EState CurrentState => _state;

        private BaseObject _hitEffect;

        #region 생명주기
        protected virtual void Start()
        {
            var sys = ServiceLocator.Instance.GetService<SystemManager>();
            sys.OnUpgradeChanged += HandleUpgradeChanged;
            if (sys.Upgrades != null && _mySlotIndex < sys.Upgrades.Length)
                ApplyUpgradeStat(sys.Upgrades[_mySlotIndex]);
        }

        private void OnDisable()
        {
            var sys = ServiceLocator.Instance?.GetService<SystemManager>();
            if (sys != null) sys.OnUpgradeChanged -= HandleUpgradeChanged;
        }
        #endregion

        #region 업그레이드 로직
        private void HandleUpgradeChanged(int slotIndex, int level)
        {
            if (slotIndex == _mySlotIndex) ApplyUpgradeStat(level);
        }

        private void ApplyUpgradeStat(int level)
        {
            if (level <= 0) return;
            if (_upgradeMod != null) _stat.Damage.RemoveModifier(_upgradeMod);
            _upgradeMod = new StatModifier(level * 15.0f, StatModType.Flat, this);
            _stat.Damage.AddModifier(_upgradeMod);
        }
        #endregion

        #region [핵심] 연출 및 타격 효과 (ApplyHitEffect)
        public void ApplyHitEffect(BaseObject hitPrefab)
        {
            if (hitPrefab == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var effect = pool.GetFromPool<BaseObject>(hitPrefab);

            if (effect != null)
            {
                effect.transform.SetParent(this.transform, false);
                effect.transform.localPosition = new Vector3(0, 0.5f, 0);
                effect.gameObject.SetActive(true);

                if (_hitEffect != null)
                {
                    _hitEffect.gameObject.SetActive(false);
                    _hitEffect.transform.SetParent(null);
                    pool.ReturnPool(_hitEffect);
                }
                _hitEffect = effect;
            }
        }

        public void DowngradEffect(BaseObject downgradePrefab, BaseObject destroyPrefab, float delay)
        {
            int beforeLank = Lank;
            Downgrade(delay);

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            BaseObject targetPrefab = (beforeLank > 1) ? downgradePrefab : destroyPrefab;

            if (targetPrefab != null)
            {
                var effect = pool.GetFromPool<BaseObject>(targetPrefab);
                if (effect != null)
                {
                    effect.gameObject.SetActive(true);
                    effect.transform.position = (beforeLank > 1) ? this.transform.position + Vector3.up * 0.5f : this.transform.position;
                    pool.StartCoroutine(ReturnEffectToPool(pool, effect, 1.5f));
                }
            }
        }

        private IEnumerator ReturnEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (effect != null && effect.gameObject.activeSelf)
            {
                effect.transform.SetParent(null);
                effect.gameObject.SetActive(false);
                pool.ReturnPool(effect);
            }
        }
        #endregion

        #region 봉인 및 랭크 시스템
        public void Setup(TowerData data, int rank, int slotIndex)
        {
            this.Lank = rank;
            this._mySlotIndex = slotIndex;
            var targetStat = data.stats.Find(s => s.rank == rank);
            if (targetStat != null && _stat != null)
            {
                _stat.Damage.BaseValue = targetStat.attack;
                _stat.Speed.BaseValue = targetStat.speed;
                _stat.Range.BaseValue = targetStat.range;
                _stat.CurrentHp = _stat.MaxHp.Value;
                for (int i = 0; i < targetStat.specialValues.Count; i++)
                    if (i < _stat.SpecialValues.Count) _stat.SpecialValues[i].BaseValue = targetStat.specialValues[i];
            }
            ApplyUpgradeStat(ServiceLocator.Instance.GetService<SystemManager>().Upgrades[_mySlotIndex]);
        }

        public void Seal(BaseObject effectPrefab)
        {
            if (_isSealed) return;
            _isSealed = true;
            if (effectPrefab != null)
            {
                var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
                var effect = pool.GetFromPool<BaseObject>(effectPrefab);
                if (effect != null)
                {
                    effect.transform.SetParent(this.transform, false);
                    effect.transform.localPosition = Vector3.zero;
                    effect.transform.localScale = Vector3.one;
                    effect.gameObject.SetActive(true);
                    _currentEffect = effect;
                }
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
                if (data != null) Setup(data, Lank, _mySlotIndex);
                if (StarUI != null) StarUI.Init(this);
            }
            else StartCoroutine(DestroyRoutine(delay));
        }

        private IEnumerator DestroyRoutine(float delay)
        {
            _isSealed = true;
            if (CurrentTile != null) CurrentTile._isUsed = false;
            yield return new WaitForSeconds(delay);
            ServiceLocator.Instance.GetService<MainManager>()?.RemoveUnit(this);
            gameObject.SetActive(false);
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        public override void OnReturnToPool() { UnSeal(); base.OnReturnToPool(); }
        #endregion

        public override void ActiveAttack() { }
        public override void ObjectUpdate() { if (_isSealed) return; base.ObjectUpdate(); }
    }
}