using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using Dev.jeon.Model;
using System.Collections;
using UnityEngine;

public class SlowBullet : BaseBullet
{
    [Header("Slow Settings")]
    [SerializeField] private float _slowAmount = 0.5f;
    [SerializeField] private float _slowDuration = 2.0f;

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

            transform.position = Vector3.MoveTowards(transform.position, lastTargetPos, _speed * Time.deltaTime);

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
            if (_target.TryGetComponent(out Enemy enemy))
            {
                enemy.OnDamaged(_damage, _fontColor);
                SpawnHitEffect(transform.position);

                var existingSlow = enemy.GetBuff<SlowBuff>();
                if (existingSlow == null)
                {
                    var slowBuff = new SlowBuff(_slowAmount);
                    // 인자 3개를 모두 전달하도록 수정
                    slowBuff.Init(enemy, _slowDuration, _hitEffectPrefab);
                    enemy.AddBuff(slowBuff);
                }
                else
                {
                    existingSlow.Refresh(_slowDuration);
                }
            }
        }
        ReturnToPool();
    }

    protected override void ReturnToPool()
    {
        ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
    }

    private void OnDisable()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _target = null;
    }

    public override void ObjectUpdate() { }
}