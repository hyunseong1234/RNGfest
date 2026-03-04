using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ElectricBullet : BaseBullet
    {
        [Header("전기 속성 설정")]
        [SerializeField] private float _speed = 20f;
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _bounceRadius = 5f;

        [Header("시각 효과")]
        [SerializeField] private LineRenderer _lineRenderer; // 🟢 인스펙터에서 할당!
        [SerializeField] private float _lightningDuration = 0.2f; // 번개가 화면에 남아있는 시간
        [SerializeField] private float _startWidth = 0.5f; // 처음 맞았을 때 번개 굵기
        [SerializeField] private float _endWidth = 0.1f;   // 마지막에 튕길 때 번개 굵기

        private int _maxTargets = 3;
        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };
        private Coroutine _moveCoroutine;

        public override void Init(Transform target, int damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            // 총알이 날아가는 동안은 선을 숨김
            if (_lineRenderer != null) _lineRenderer.enabled = false;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            while (_target != null && _target.gameObject.activeSelf)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _target.transform.position,
                    _speed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, _target.transform.position) < 0.05f)
                {
                    // 🟢 즉시 반납하지 않고, 번개 연출 코루틴을 시작합니다!
                    StartCoroutine(HitAndDrawLightning());
                    yield break;
                }

                yield return null;
            }

            ReturnToPool();
        }

        // 🟢 데미지를 주고 선을 그리는 코루틴
        private IEnumerator HitAndDrawLightning()
        {
            var primaryEnemy = _target.GetComponent<Enemy>();
            if (primaryEnemy == null)
            {
                ReturnToPool();
                yield break;
            }

            List<Enemy> hitEnemies = new List<Enemy>();
            List<Vector3> linePoints = new List<Vector3>(); // 선을 그릴 좌표들

            Enemy currentTarget = primaryEnemy;

            // 선의 시작점은 현재 총알의 위치(첫 번째 타겟 위치)
            linePoints.Add(transform.position);

            for (int i = 0; i < _maxTargets; i++)
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf) break;

                // 1. 데미지 적용
                int finalDamage = Mathf.RoundToInt(_damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage);
                hitEnemies.Add(currentTarget);

                // 2. 선을 이을 다음 좌표 추가 (몬스터의 위치)
                linePoints.Add(currentTarget.transform.position);

                // 3. 다음 타겟 찾기
                if (i < _maxTargets - 1)
                {
                    Enemy nextTarget = FindNextTarget(currentTarget, hitEnemies);
                    if (nextTarget == null) break;
                    currentTarget = nextTarget;
                }
            }

            // 🟢 시각 효과 그리기 (LineRenderer 세팅)
            if (_lineRenderer != null && linePoints.Count > 1)
            {
                _lineRenderer.positionCount = linePoints.Count;
                _lineRenderer.SetPositions(linePoints.ToArray());

                // 갈수록 얇아지는 연출 적용 (시작 굵기와 끝 굵기 다르게 설정)
                _lineRenderer.startWidth = _startWidth;
                _lineRenderer.endWidth = _endWidth;

                _lineRenderer.enabled = true; // 선 켜기
            }

            // 번개가 번쩍! 하고 화면에 잠시 남아있도록 대기
            yield return new WaitForSeconds(_lightningDuration);

            // 대기 후 선 끄고 총알 풀로 반납
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            ReturnToPool();
        }

        private Enemy FindNextTarget(Enemy currentEnemy, List<Enemy> alreadyHit)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager == null) return null;

            return mainManager.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf)
                .Where(e => !alreadyHit.Contains(e))
                .Where(e => (e.transform.position - currentEnemy.transform.position).sqrMagnitude <= (_bounceRadius * _bounceRadius))
                .OrderBy(e => (e.transform.position - currentEnemy.transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        private void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        private void OnDisable()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            _target = null;
        }

        public override void ObjectUpdate() { }
    }
}