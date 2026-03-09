using Dev.cheol.Manager;
using Dev.cheol.Model;
using UnityEditor.EditorTools;
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
                // 1. 위치 설정 (적의 중심부 근처)
                explosionEffect.transform.position = target.transform.position + new Vector3(0, 0.5f, 0);

                // 2. 기본 방향 설정 (적이 타워를 바라보게)
                // 만약 BaseBullet에 타워 참조가 없다면 사전에 캐싱하거나 Init 인자로 전달받아야 합니다.
                Vector3 directionToTower = (this.transform.position - target.transform.position).normalized;
                directionToTower.y = 0; // y축 회전만 고려

                if (directionToTower != Vector3.zero)
                {
                    // 이펙트가 타워 쪽을 정면으로 바라보게 함
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTower);

                    // 3. 종베기 랜덤 회전값 계산 (Z축 기준 세로 베기)
                    // 검기가 '종베기(세로)' 형식이라면, Z축을 기준으로 약간씩 틀어줘야 랜덤한 세로 베기 각도가 나옵니다.
                    // (만약 횡베기라면 X축이나 Y축 기준이어야 합니다.)
                    float verticalSlashTilt = Random.Range(-20f, 20f); // 세로 각도를 +-20도 사이로 랜덤하게 틈
                    Quaternion randomTilt = Quaternion.Euler(0, 0, verticalSlashTilt);

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
