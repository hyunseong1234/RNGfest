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
        [SerializeField] private float _speed = 20f;
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _bounceRadius = 5f; // 번개가 튕길 수 있는 사거리

        [Header("번개 시각 효과 설정")]
        [SerializeField] private LineRenderer _lineRenderer; // 🟢 유니티 인스펙터에서 연결!
        [SerializeField] private float _lightningDuration = 0.2f; // 번개가 화면에 남는 시간
        [SerializeField] private float _startWidth = 0.5f; // 첫 타겟에 맞는 번개 굵기
        [SerializeField] private float _endWidth = 0.1f;   // 튕길수록 얇아지는 굵기

        private int _maxTargets = 3; // 튕기는 횟수
        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };
        private Coroutine _moveCoroutine;

        public override void Init(Transform target, int damage, float speed = 20)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            // 🟢 날아가는 동안은 선을 숨깁니다.
            if (_lineRenderer != null) _lineRenderer.enabled = false;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            while (_target != null && _target.gameObject.activeSelf)
            {
                transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, _target.transform.position) < 0.05f)
                {
                    // 🟢 기존의 HitTarget() 대신, 데미지+시각효과를 동시에 처리하는 코루틴 실행
                    StartCoroutine(HitAndDrawLightning());
                    yield break;
                }
                yield return null;
            }

            ReturnToPool();
        }

        // 🟢 여기가 핵심! 데미지를 주고 선을 그립니다.
        private IEnumerator HitAndDrawLightning()
        {
            var primaryEnemy = _target.GetComponent<Enemy>();
            if (primaryEnemy == null)
            {
                ReturnToPool();
                yield break;
            }

            List<Enemy> hitEnemies = new List<Enemy>();
            List<Vector3> linePoints = new List<Vector3>(); // 선이 꺾일 좌표들 저장

            Enemy currentTarget = primaryEnemy;

            // 번개의 시작점 (현재 총알 위치)
            linePoints.Add(transform.position);

            // 1. 최대 3마리 타격 루프 (데미지 계산 및 좌표 수집)
            for (int i = 0; i < _maxTargets; i++)
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf) break;

                // 순서에 맞는 데미지 계산 및 적용
                int finalDamage = Mathf.RoundToInt(_damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage,_fontColor);
                hitEnemies.Add(currentTarget);

                //  몬스터가 맞은 위치를 번개가 지나갈 좌표에 추가
                linePoints.Add(currentTarget.transform.position);

                Debug.Log($"<color=cyan>[체인 라이트닝]</color> {i + 1}번째 타겟({currentTarget.name}) 적중! 데미지: {finalDamage}");

                // 다음 타겟 찾기
                if (i < _maxTargets - 1)
                {
                    Enemy nextTarget = FindNextTarget(currentTarget, hitEnemies);
                    if (nextTarget == null) break;
                    currentTarget = nextTarget;
                }
            }

            // 2. 수집된 좌표들을 바탕으로 번개(선) 그리기
            if (_lineRenderer != null && linePoints.Count > 1)
            {
                _lineRenderer.positionCount = linePoints.Count;
                _lineRenderer.SetPositions(linePoints.ToArray());

                // 갈수록 얇아지는 연출 적용
                _lineRenderer.startWidth = _startWidth;
                _lineRenderer.endWidth = _endWidth;

                _lineRenderer.enabled = true; // 선 보이기
            }

            // 3. 번개가 번쩍! 하고 잠시 남아있도록 대기
            yield return new WaitForSeconds(_lightningDuration);

            // 4. 대기가 끝나면 선을 지우고 총알 반납
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