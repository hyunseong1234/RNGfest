using Dev.cheol.Manager;
using Dev.cheol.Model;
using UnityEngine;

public class TargetingBullet : BaseBullet
{

    [SerializeField] private BaseObject _explosionEffectPrefab;

    public override void Init(Transform target, float damage, float speed = 20)
    {
        _target = target;
        _damage = damage;
        _speed = speed;

        //TODO : 마감 끝나고 리팩토링 할 예정
        #region 함수화 시켜야되는 구간

        var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        if (poolManager != null && _explosionEffectPrefab != null && target != null)
        {
            var explosionEffect = poolManager.GetFromPool<BaseObject>(_explosionEffectPrefab);
            if (explosionEffect != null)
            {

                explosionEffect.transform.position = target.transform.position + new Vector3(0, 0.5f, 0);

                Vector3 directionToTower = (this.transform.position - target.transform.position).normalized;
                directionToTower.y = 0; // y축 회전만 고려

                if (directionToTower != Vector3.zero)
                {
                    // 이펙트가 타워 쪽을 정면으로 바라보게 함
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTower);


                    float verticalSlashTilt = Random.Range(-20f, 20f); // 세로 각도를 +-20도 사이로 랜덤하게 틈
                    Quaternion randomTilt = Quaternion.Euler(0, verticalSlashTilt, 0);

                    // 4. 최종 회전값 적용 (방향 * 랜덤 기울기)
                    explosionEffect.transform.rotation = lookRotation * randomTilt;
                }
            }
        }

        #endregion

        if (target.TryGetComponent(out Enemy enemy))
        {
            enemy.OnDamaged(_damage, _fontColor);
        }
    }

    public override void ObjectUpdate()
    {
    }


}
