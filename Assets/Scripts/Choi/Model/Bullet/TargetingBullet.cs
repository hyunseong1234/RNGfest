using Dev.cheol.Manager;
using Dev.cheol.Model;
using UnityEngine;

public class TargetingBullet : BaseBullet
{
    // TODO : basebullet 수정 과정으로 인하여 마감 끝나고 변경
    //[SerializeField] private BaseObject _explosionEffectPrefab;

    //public override void Init(Transform target, float damage, float speed = 20)
    //{
    //    _target = target;
    //    _damage = damage;
    //    _speed = speed;

    //    //TODO : 마감 끝나고 리팩토링 할 예정
    //    #region 함수화 시켜야되는 구간

    //    var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
    //    if (poolManager != null && _explosionEffectPrefab != null && target != null)
    //    {
    //        var explosionEffect = poolManager.GetFromPool<BaseObject>(_explosionEffectPrefab);
    //        if (explosionEffect != null)
    //        {

    //            explosionEffect.transform.position = target.transform.position + new Vector3(0, 0.5f, 0);

    //            Vector3 directionToTower = (this.transform.position - target.transform.position).normalized;
    //            directionToTower.y = 0; // y축 회전만 고려

    //            if (directionToTower != Vector3.zero)
    //            {
    //                // 이펙트가 타워 쪽을 정면으로 바라보게 함
    //                Quaternion lookRotation = Quaternion.LookRotation(directionToTower);


    //                float verticalSlashTilt = Random.Range(-20f, 20f); // 세로 각도를 +-20도 사이로 랜덤하게 틈
    //                Quaternion randomTilt = Quaternion.Euler(0, verticalSlashTilt, 0);

    //                // 4. 최종 회전값 적용 (방향 * 랜덤 기울기)
    //                explosionEffect.transform.rotation = lookRotation * randomTilt;
    //            }
    //        }
    //    }

    //    #endregion

    //    if (target.TryGetComponent(out Enemy enemy))
    //    {
    //        enemy.OnDamaged(_damage, _fontColor);
    //    }
    //}
    protected override void StartMove()
    {
        // 이동하지 않고 즉시 적중 처리
        OnHit(_target.position + new Vector3(0, 0.5f, 0));
    }

    protected override void ApplyHitLogic(Vector3 hitPoint)
    {
        if (_target != null && _target.TryGetComponent(out Enemy enemy))
        {
            enemy.OnDamaged(_damage, _fontColor);
            // 슬래시 이펙트 회전 로직은 여기서 처리 (부모의 SpawnHitEffect 이후 추가 연출)
        }
    }
    protected override void SpawnHitEffect(Vector3 position)
    {
        if (_hitEffectPrefab == null || _target == null) return;

        var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        BaseObject effect = pool.GetFromPool<BaseObject>(_hitEffectPrefab);

        if (effect != null)
        {
            effect.transform.position = position;
            Vector3 directionToTower = (this.transform.position - _target.position).normalized;
            directionToTower.y = 0;

            if (directionToTower != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToTower);
                float verticalSlashTilt = Random.Range(-20f, 20f);
                effect.transform.rotation = lookRotation * Quaternion.Euler(0, 0, verticalSlashTilt);
            }
            effect.gameObject.SetActive(true);
        }
    }
    public override void ObjectUpdate()
    {
    }


}
