using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ElectricityBullet : BaseBullet
    {
        [Header("전기 속성 설정")]
        [SerializeField] private float _bounceRadius = 5f;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _lightningDuration = 0.2f;

        private int _maxTargets = 3;
        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };

        public override void Init(Transform target, float damage, float speed = 20)
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            base.Init(target, damage, speed); // 부모 초기화 및 이동 시작
        }

        // 전기는 번개 연출 시간 때문에 반납 타이밍을 직접 제어해야 합니다.
        protected override IEnumerator MoveToTargetRoutine()
        {
            // 부모의 이동 로직을 쓰되, OnHit 대신 직접 연출 코루틴 실행
            yield return base.MoveToTargetRoutine();
        }

        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            // 이 친구는 특수 연출(코루틴)을 통해 데미지를 주므로 여기선 메인 코루틴만 시작합니다.
            StartCoroutine(HitAndDrawLightning(hitPoint));
        }

        private IEnumerator HitAndDrawLightning(Vector3 hitPosition)
        {
            Enemy primaryEnemy = (_target != null && _target.gameObject.activeSelf) ? _target.GetComponent<Enemy>() : null;
            List<Enemy> hitEnemies = new List<Enemy>();
            List<Vector3> linePoints = new List<Vector3> { hitPosition };

            Enemy currentTarget = primaryEnemy;

            for (int i = 0; i < _maxTargets; i++)
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf)
                    currentTarget = FindNextTarget(linePoints.Last(), hitEnemies);

                if (currentTarget == null) break;

                // 데미지 및 이펙트
                int finalDamage = Mathf.RoundToInt(_damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage, _fontColor);
                SpawnHitEffect(currentTarget.transform.position);

                // 튕길 때마다 전기 사운드 재생 (부모 함수 활용)
                PlaySound(_hitSound);

                hitEnemies.Add(currentTarget);
                linePoints.Add(currentTarget.transform.position);

                if (i < _maxTargets - 1)
                    currentTarget = FindNextTarget(currentTarget.transform.position, hitEnemies);
            }

            if (_lineRenderer != null && linePoints.Count > 1)
            {
                _lineRenderer.positionCount = linePoints.Count;
                _lineRenderer.SetPositions(linePoints.ToArray());
                _lineRenderer.enabled = true;
            }

            yield return new WaitForSeconds(_lightningDuration);
            if (_lineRenderer != null) _lineRenderer.enabled = false;

            // 모든 연출이 끝나면 수동으로 반납
            ReturnToPool();
        }

        private Enemy FindNextTarget(Vector3 pos, List<Enemy> alreadyHit)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            return mainManager?.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf && !alreadyHit.Contains(e))
                .Where(e => (e.transform.position - pos).sqrMagnitude <= (_bounceRadius * _bounceRadius))
                .OrderBy(e => (e.transform.position - pos).sqrMagnitude)
                .FirstOrDefault();
        }
    }
}