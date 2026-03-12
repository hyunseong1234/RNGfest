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

        [SerializeField] private float _bounceRadius = 5f; // 번개가 튕길 수 있는 사거리

        [Header("번개 시각 효과 설정")]
        [SerializeField] private LineRenderer _lineRenderer; // 🟢 유니티 인스펙터에서 연결!
        [SerializeField] private float _lightningDuration = 0.2f; // 번개가 화면에 남는 시간
        [SerializeField] private float _startWidth = 0.5f; // 첫 타겟에 맞는 번개 굵기
        [SerializeField] private float _endWidth = 0.1f;   // 튕길수록 얇아지는 굵기

        private int _maxTargets = 3; // 튕기는 횟수
        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };
        private Coroutine _moveCoroutine;

        public override void Init(Transform target, float damage, float speed = 20)
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
            // 1. 타겟의 마지막 위치 기억
            Vector3 lastTargetPos = _target != null ? _target.position : transform.position;

            // 2. 타겟 생존 여부와 상관없이 '도착할 때까지' 루프 (NormalBullet 방식)
            while (true)
            {
                if (_target != null && _target.gameObject.activeSelf)
                {
                    lastTargetPos = _target.position;
                }

                // 마지막으로 확인된 위치를 향해 이동
                transform.position = Vector3.MoveTowards(transform.position, lastTargetPos, _speed * Time.deltaTime);

                // 3. 도착 체크
                if (Vector3.Distance(transform.position, lastTargetPos) < 0.05f)
                {
                    yield return StartCoroutine(HitAndDrawLightning(lastTargetPos));

                    yield break;
                }
                yield return null;
            }

        }

        //  데미지를 주고 선을 그립니다.
        private IEnumerator HitAndDrawLightning(Vector3 hitPosition)
        {
            // [수정 1] _target이 null일 때를 대비해 안전하게 컴포넌트를 가져옵니다.
            Enemy primaryEnemy = (_target != null && _target.gameObject.activeSelf) ? _target.GetComponent<Enemy>() : null;

            // [수정 2] primaryEnemy가 없다고 바로 종료(yield break)하지 않습니다!
            // 아래 for문에서 주변 적을 새로 찾는 로직이 커버해줄 겁니다.

            List<Enemy> hitEnemies = new List<Enemy>();
            List<Vector3> linePoints = new List<Vector3>();

            // 번개의 시작점 (총알이 도착한 좌표)
            linePoints.Add(hitPosition);

            Enemy currentTarget = primaryEnemy;

            for (int i = 0; i < _maxTargets; i++)
            {
                // [수정 3] 현재 타겟이 없다면? 주변에서 가장 가까운 적을 새로 검색합니다.
                if (currentTarget == null || !currentTarget.gameObject.activeSelf)
                {
                    // linePoints의 마지막 위치(방금 번개가 도달한 곳)를 기준으로 검색
                    currentTarget = FindNextTarget(linePoints[linePoints.Count - 1], hitEnemies);
                }

                // 그래도 주변에 아무도 없다면 루프 종료
                if (currentTarget == null) break;

                // 데미지 적용
                int finalDamage = Mathf.RoundToInt(_damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage, _fontColor);
                SpawnHitEffect(currentTarget.transform.position);
                hitEnemies.Add(currentTarget);

                // 궤적 추가
                linePoints.Add(currentTarget.transform.position);

                Debug.Log($"<color=cyan>[체인 라이트닝]</color> {i + 1}번째 타겟({currentTarget.name}) 적중!");

                // 다음 타겟 미리 찾기 준비
                if (i < _maxTargets - 1)
                {
                    currentTarget = FindNextTarget(currentTarget.transform.position, hitEnemies);
                }
            }

            // [수정 4] 번개 그리기 (좌표가 2개 이상이면 무조건 그립니다)
            if (_lineRenderer != null && linePoints.Count > 1)
            {
                _lineRenderer.positionCount = linePoints.Count;
                _lineRenderer.SetPositions(linePoints.ToArray());
                _lineRenderer.startWidth = _startWidth;
                _lineRenderer.endWidth = _endWidth;
                _lineRenderer.enabled = true;
            }

            yield return new WaitForSeconds(_lightningDuration);

            if (_lineRenderer != null) _lineRenderer.enabled = false;
            ReturnToPool();
        }

        private Enemy FindNextTarget(Vector3 pos, List<Enemy> alreadyHit)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager == null) return null;

            return mainManager.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf)
                .Where(e => !alreadyHit.Contains(e))
                .Where(e => (e.transform.position - pos).sqrMagnitude <= (_bounceRadius * _bounceRadius))
                .OrderBy(e => (e.transform.position - pos).sqrMagnitude)
                .FirstOrDefault();
        }

        protected override void ReturnToPool()
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