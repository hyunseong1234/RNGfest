using Dev.cheol.Manager;
using Dev.cheol.Model; // BaseObject나 Enemy가 있는 네임스페이스
using System.Collections;
using UnityEngine;

public class NormalBullet : BaseBullet
{

    private Coroutine _moveCoroutine;

    // 풀에서 꺼낼 때 호출할 초기화 함수
    public override void Init(Transform target, float damage, float speed = 20f)
    {
        _target = target;
        _damage = damage;
        _speed = speed;

        // 기존에 돌던 코루틴이 있다면 방어적으로 중지
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        // 1. 타겟의 마지막 위치를 계속 기억할 변수
        Vector3 lastTargetPos = _target.position;
        // 타겟이 활성화되어 있는 동안 계속 추적
        // 2. 루프 조건: 타겟 생존 여부와 상관없이 '도착할 때까지' 무한 루프
        while (true) 
        {
            // 타겟이 살아있고 활성화되어 있다면 매 프레임 위치를 갱신 (추적 기능)
            if (_target != null && _target.gameObject.activeSelf)
            {
                lastTargetPos = _target.position;
            }

            // 3. '마지막으로 확인된 위치'를 향해 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                lastTargetPos,
                _speed * Time.deltaTime
            );

            // 4. 도착 체크 (타겟 오브젝트가 아니라 '저장된 위치 좌표'와 비교)
            if (Vector3.Distance(transform.position, lastTargetPos) < 0.01f)
            {
                HitTarget(); // 도착했으니 데미지 판정 시도
                yield break; // 코루틴 종료
            }

            yield return null;
        }
    }

    private void HitTarget()
    {
        // 타겟의 Enemy 컴포넌트나 BaseObject를 가져와 데미지 입힘
        var enemy = _target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.OnDamaged(_damage, 0);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {

        ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 코루틴을 확실히 멈춰서 에러 방지
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        _target = null;
    }

    public override void ObjectUpdate()
    {
        throw new System.NotImplementedException();
    }
}