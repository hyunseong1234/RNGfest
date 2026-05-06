using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    /// <summary>
    /// 전기 총알
    /// 이동 담당 → 착탄 후 LightningChain에 연쇄 로직 위임
    /// </summary>
    public class ElectricityBullet : BaseBullet, IAbilityBoost
    {
        // 연쇄 로직은 LightningChain이 담당
        private LightningChain _lightningChain;

        protected override void Awake()
        {
            base.Awake();
            _lightningChain = GetComponent<LightningChain>();
        }

        #region 증강
        /// <summary>
        /// 증강 적용 → LightningChain에 연쇄 횟수 증가 위임
        /// </summary>
        public void ApplyAbilityBoost(float value)
        {
            int addCount = Mathf.RoundToInt(value);
            _lightningChain?.AddChainCount(addCount);
            Debug.Log($"[ElectricityBullet] 연쇄 횟수 증가: {_lightningChain?.MaxTargets}");
        }
        #endregion

        #region BaseBullet override
        public override void Init(Transform target, float damage, float speed = 20)
        {
            base.Init(target, damage, speed); // 부모 초기화 및 이동 시작
        }

        /// <summary>
        /// 이동 → 착탄 → LightningChain 실행 → 반납
        /// </summary>
        protected override IEnumerator MoveToTargetRoutine()
        {
            // 1. 부모 이동 로직으로 타겟까지 이동
            Vector3 lastPos = _target != null ? _target.position : transform.position;
            while (true)
            {
                if (_target != null && _target.gameObject.activeSelf) lastPos = _target.position;
                transform.position = Vector3.MoveTowards(transform.position, lastPos, _speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, lastPos) < 0.05f)
                    break;

                yield return null;
            }

            // 2. 착탄 히트 이펙트
            SpawnHitEffect(lastPos);

            // 3. 연쇄 로직을 LightningChain에 위임
            Enemy firstTarget = _target != null ? _target.GetComponent<Enemy>() : null;
            if (_lightningChain != null)
                yield return StartCoroutine(_lightningChain.Execute(lastPos, firstTarget, _damage, _fontColor, _hitSound, this));

            // 4. 반납
            ReturnToPool();
        }

        /// <summary>
        /// MoveToTargetRoutine에서 직접 처리하므로 비워둠
        /// </summary>
        protected override void ApplyHitLogic(Vector3 hitPoint) { }
        #endregion
    }
}