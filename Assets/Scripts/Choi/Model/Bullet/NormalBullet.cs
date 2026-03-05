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
        // 타겟이 활성화되어 있는 동안 계속 추적
        while (_target != null && _target.gameObject.activeSelf)
        {
            // MoveTowards로 부드럽게 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                _target.position,
                _speed * Time.deltaTime
            );

            // 타겟과의 거리가 아주 가까워지면 충돌 처리
            if (Vector3.Distance(transform.position, _target.position) < 0.05f)
            {
                HitTarget();
                yield break; // 코루틴 종료
            }

            yield return null;
        }

        // 타겟이 사라지거나 비활성화되면 총알도 제거(풀 반납)
        ReturnToPool();
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