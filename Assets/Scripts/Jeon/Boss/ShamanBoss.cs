using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet; // SkillBullet 사용
using Dev.jeon.Model;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class ShamanBoss : BaseBoss // 주술사 보스
    {
        [Header("Projectile Settings")]
        // [수정] 문자열(Key) 대신 프리팹을 직접 연결할 수 있는 슬롯 생성!
        [SerializeField] private SkillBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 1.5f; // 포물선 도달 시간

        protected override void ApplySkillEffect()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var towers = main.SpawnTowers;

            if (towers == null || towers.Count == 0) return;

            // 1. 타워 리스트를 랜덤하게 섞음
            var randomTowers = towers.OrderBy(x => Random.value).ToList();

            // 2. 활성화된 타워 개수의 절반 계산 (최소 1개)
            int targetCount = Mathf.Max(1, randomTowers.Count / 2);

            // 3. 절반만큼 투사체(포물선 탄) 발사!
            for (int i = 0; i < targetCount; i++)
            {
                Tower target = randomTowers[i];

                // [수정] 오브젝트 풀에서 '프리팹'을 기준으로 스킬 탄 가져오기
                var bullet = pool.GetFromPool<SkillBullet>(_bulletPrefab);

                if (bullet != null)
                {
                    // 보스 머리 위에서 발사
                    bullet.transform.position = transform.position + Vector3.up * 2f;

                    // 스킬 타입을 SHAMAN으로 넘겨서 포물선으로 날아가게 함
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.SHAMAN);
                }
            }

            Debug.Log($"[주술사 보스] 타워 {targetCount}개를 향해 저주의 포물선 탄 발사!");
        }
    }
}