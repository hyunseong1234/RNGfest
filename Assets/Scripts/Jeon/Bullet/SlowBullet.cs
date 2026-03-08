using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats; // SlowBuff와 Buff 시스템이 있는 곳
using Dev.jeon.Model;
using System.Collections;
using UnityEngine;

public class SlowBullet : BaseBullet
{
    [Header("Slow Settings")]
    [SerializeField] private float _slowAmount = 0.5f;  // 감속 비율 (50%)
    [SerializeField] private float _slowDuration = 2.0f; // 감속 지속 시간

    private Coroutine _moveCoroutine;

    public override void Init(Transform target, float damage, float speed = 20f)
    {
        _target = target;
        _damage = damage;
        _speed = speed;

        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 lastTargetPos = _target.position;

        while (true)
        {
            if (_target != null && _target.gameObject.activeSelf)
            {
                lastTargetPos = _target.position;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                lastTargetPos,
                _speed * Time.deltaTime
            );

            // 도착 체크
            if (Vector3.Distance(transform.position, lastTargetPos) < 0.05f)
            {
                HitTarget();
                yield break;
            }

            yield return null;
        }
    }

    private void HitTarget()
    {
        if (_target != null && _target.gameObject.activeSelf)
        {
            var enemy = _target.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 1. 기본 데미지 입힘
                enemy.OnDamaged(_damage, _fontColor);

                // 2. [SlowZone 참고] 슬로우 버프 적용 로직
                var existingSlow = enemy.GetBuff<SlowBuff>();

                if (existingSlow == null)
                {
                    // 버프가 없으면 새로 생성해서 추가
                    var slowBuff = new SlowBuff(_slowAmount);
                    slowBuff.Init(enemy, _slowDuration);
                    enemy.AddBuff(slowBuff);
                }
                else
                {
                    // 이미 있다면 시간만 갱신 (Refresh)
                    // 필요하다면 Refresh 인자에 _slowDuration을 전달하도록 SlowBuff를 수정하세요.
                    existingSlow.Refresh(_slowDuration);
                }
            }
        }

        ReturnToPool();
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
        _target = null;
    }

    public override void ObjectUpdate() { }
}