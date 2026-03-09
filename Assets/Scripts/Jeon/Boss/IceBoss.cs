using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using Dev.jeon.Bullet;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class IceBoss : BaseBoss
    {
        [Header("Projectile Settings")]
        // [수정] 인스펙터에서 직접 드래그할 수 있게 프리팹 슬롯으로 변경 (선택사항이나 권장)
        [SerializeField] private SkillBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;

        // [수정] 부모 클래스의 추상 함수 이름과 리턴 타입에 맞춤
        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (main.SpawnTowers == null || main.SpawnTowers.Count == 0) yield break;

            // 봉인되지 않은 타워 중 랜덤 선택
            var availableTowers = main.SpawnTowers.FindAll(t => !t.IsSealed);

            if (availableTowers.Count > 0)
            {
                int rand = Random.Range(0, availableTowers.Count);
                Tower target = availableTowers[rand];

                // 프리팹 혹은 키값을 사용하여 풀에서 탄환 소환
                var bullet = pool.GetFromPool<SkillBullet>(_bulletPrefab);

                if (bullet != null)
                {
                    bullet.transform.position = transform.position + Vector3.up * 2f;

                    // ICE 타입으로 초기화 (직선 비행)
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.ICE);

                    Debug.Log($"[빙결 보스] {target.name}에게 빙결탄 발사! (타일 기반 발동)");
                }
            }

            // 탄환 발사는 즉시 일어나므로 한 프레임 대기 후 종료
            yield return null;
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            // 타일 카운터 초기화 (부모 클래스 변수)
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}