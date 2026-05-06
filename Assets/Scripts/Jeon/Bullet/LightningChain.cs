using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    /// <summary>
    /// 연쇄 번개 로직 담당
    /// ElectricityBullet에서 호출하거나
    /// 전기 궁극기에서도 재사용 가능
    /// </summary>
    public class LightningChain : MonoBehaviour
    {
        [Header("연쇄 설정")]
        [SerializeField] private float _bounceRadius = 5f;
        [SerializeField] private float _lightningDuration = 0.2f;
        [SerializeField] private LineRenderer _lineRenderer;

        private int _maxTargets = 3;
        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };

        private MainManager _mainManager;

        private void Awake()
        {
            _mainManager = ServiceLocator.Instance.GetService<MainManager>();
        }

        /// <summary>
        /// 연쇄 횟수 증가 (증강에서 호출)
        /// </summary>
        public void AddChainCount(int count)
        {
            _maxTargets += count;

            float[] newMultipliers = new float[_maxTargets];
            for (int i = 0; i < _maxTargets; i++)
            {
                if (i < _damageMultipliers.Length)
                    newMultipliers[i] = _damageMultipliers[i];
                else
                    newMultipliers[i] = Mathf.Max(0.1f, _damageMultipliers[_damageMultipliers.Length - 1] - 0.1f);
            }
            _damageMultipliers = newMultipliers;
        }

        public int MaxTargets => _maxTargets;

        /// <summary>
        /// 연쇄 번개 실행
        /// ElectricityBullet, 전기 궁극기 등에서 호출
        /// </summary>
        public IEnumerator Execute(Vector3 startPos, Enemy firstTarget, float damage, FontColor fontColor, AudioClip hitSound, BaseBullet caller)
        {
            List<Enemy> hitEnemies = new List<Enemy>();
            List<Vector3> linePoints = new List<Vector3> { startPos };
            Enemy currentTarget = firstTarget;

            for (int i = 0; i < _maxTargets; i++)
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf)
                    currentTarget = FindNextTarget(linePoints.Last(), hitEnemies);
                if (currentTarget == null) break;

                // 데미지
                int finalDamage = Mathf.RoundToInt(damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage, fontColor);

                // 이펙트 + 사운드 (caller의 부모 함수 활용)
                caller.SpawnHitEffect(currentTarget.transform.position);
                caller.PlaySound(hitSound);

                hitEnemies.Add(currentTarget);
                linePoints.Add(currentTarget.transform.position);

                if (i < _maxTargets - 1)
                    currentTarget = FindNextTarget(currentTarget.transform.position, hitEnemies);
            }

            // 라인렌더러 연출
            if (_lineRenderer != null && linePoints.Count > 1)
            {
                _lineRenderer.positionCount = linePoints.Count;
                _lineRenderer.SetPositions(linePoints.ToArray());
                _lineRenderer.enabled = true;
            }

            yield return new WaitForSeconds(_lightningDuration);

            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }

        /// <summary>
        /// 이미 맞은 적 제외하고 범위 내 가장 가까운 다음 타겟 탐색
        /// </summary>
        private Enemy FindNextTarget(Vector3 pos, List<Enemy> alreadyHit)
        {
            return _mainManager?.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf && !alreadyHit.Contains(e))
                .Where(e => (e.transform.position - pos).sqrMagnitude <= (_bounceRadius * _bounceRadius))
                .OrderBy(e => (e.transform.position - pos).sqrMagnitude)
                .FirstOrDefault();
        }
    }
}