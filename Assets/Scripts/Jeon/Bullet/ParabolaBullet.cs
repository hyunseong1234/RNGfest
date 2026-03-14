using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ParabolaBullet : BaseBullet
    {
        [Header("포물선 설정")]
        [SerializeField] protected float _arcHeight = 5f;

        // 1. 초기화 (부모의 Init을 활용하여 발사음 자동 재생)
        public override void Init(Transform target, float damage, float speed = 20f)
        {
            // 부모의 Init을 호출 (발사음 재생 및 기본 변수 세팅)
            base.Init(target, damage, speed);
        }

        // 2. 이동 방식 정의 (부모의 선형 이동 대신 포물선 코루틴 실행)
        protected override void StartMove()
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        // 3. 필수 구현 (컴파일 에러 해결 구간)
        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            // 타겟이 살아있다면 데미지 적용
            if (_target != null && _target.TryGetComponent(out Enemy enemy))
            {
                enemy.OnDamaged(_damage, _fontColor);
            }
        }

        private IEnumerator MoveToTarget()
        {
            Vector3 startPos = transform.position;
            Vector3 lastTargetPos = _target != null ? _target.position : transform.position;

            float distance = Vector3.Distance(startPos, lastTargetPos);

            // 안전장치: 거리가 너무 짧으면 즉시 타격
            if (distance < 0.1f)
            {
                OnHit(lastTargetPos);
                yield break;
            }

            float totalTime = distance / _speed;
            float elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalTime; // 0 ~ 1 비율

                if (_target != null && _target.gameObject.activeSelf)
                    lastTargetPos = _target.position;

                // 1. 현재 프레임의 위치 계산 (Lerp + Sin 곡선)
                Vector3 currentPos = Vector3.Lerp(startPos, lastTargetPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * _arcHeight;

                // 2. 회전 로직 (미래의 위치를 바라보게 함)
                float nextT = Mathf.Clamp01((elapsedTime + 0.05f) / totalTime);
                Vector3 nextPos = Vector3.Lerp(startPos, lastTargetPos, nextT);
                nextPos.y += Mathf.Sin(nextT * Mathf.PI) * _arcHeight;

                Vector3 moveDirection = nextPos - currentPos;
                if (moveDirection != Vector3.zero)
                {
                    // 총알의 '위(Up)' 방향을 진행 방향으로 설정 (모델에 따라 Forward로 바꿀 수 있음)
                    transform.up = moveDirection;
                }

                // 3. 위치 이동
                transform.position = currentPos;
                yield return null;
            }

            // 4. [중요] 도착 시 부모의 OnHit 호출!
            // 여기서 사운드 재생, 이펙트 생성, ApplyHitLogic 실행, 풀 반납이 모두 일어납니다.
            OnHit(lastTargetPos);
        }

        // 부모의 OnDisable을 그대로 사용 (코루틴 중지 및 타겟 초기화 포함)
    }
}