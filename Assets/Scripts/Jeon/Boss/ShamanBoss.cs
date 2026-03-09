using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet;
using Dev.jeon.Model;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class ShamanBoss : BaseBoss
    {
        [Header("Curse Settings")]
        [SerializeField] private SkillBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 1.5f;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            CastCurse();
            yield return null;
        }

        private void CastCurse()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var towers = main.SpawnTowers;

            // 1. 필드에 타워가 없으면 즉시 종료
            if (towers == null || towers.Count == 0) return;

            // 2. 랜덤하게 리스트 섞기
            var randomTowers = towers.OrderBy(x => Random.value).ToList();

            // [수정 포인트] 전체 타워 개수와 2개 중 작은 값을 선택 (타워가 1개면 1개만, 2개 이상이면 2개 선택)
            int targetCount = Mathf.Min(2, randomTowers.Count);

            for (int i = 0; i < targetCount; i++)
            {
                Tower target = randomTowers[i];
                var bullet = pool.GetFromPool<SkillBullet>(_bulletPrefab);

                if (bullet != null)
                {
                    bullet.transform.position = transform.position + Vector3.up * 2f;

                    // 타일 타겟팅 로직이 포함된 InitSkill 호출
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.SHAMAN);
                }
            }

            Debug.Log($"[주술사] {targetCount}개의 타워에 저주 발사! (타일 기반)");
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}