using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using Dev.jeon.Bullet; // SkillBullet 사용을 위해 추가
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class IceBoss : BaseBoss
    {
        [Header("Projectile Settings")]
        [SerializeField] private string _bulletKey = "IceSkillBullet"; // 풀링용 키값
        [SerializeField] private float _bulletSpeed = 15f;

        protected override void ApplySkillEffect()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (main.SpawnTowers == null || main.SpawnTowers.Count == 0) return;

            // 봉인되지 않은 타워 찾기
            var availableTowers = main.SpawnTowers.FindAll(t => !t.IsSealed);

            if (availableTowers.Count > 0)
            {
                int rand = Random.Range(0, availableTowers.Count);
                Tower target = availableTowers[rand];

                // 1. 오브젝트 풀에서 스킬 탄 가져오기
                var bullet = pool.GetFromPool<SkillBullet>(_bulletKey);

                if (bullet != null)
                {
                    // 2. 발사 위치 설정 (보스의 위치 + 머리 위 오프셋)
                    bullet.transform.position = transform.position + Vector3.up * 2f;

                    // 3. 총알 초기화 및 발사 (타겟, 속도, 스킬 타입 전달)
                    // 이제 총알이 날아가서 맞을 때 tower.Seal()을 호출하게 됩니다.
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.ICE);

                    Debug.Log($"[빙결 보스] {target.name}에게 얼음 구체를 발사했습니다!");
                }
            }
        }
    }
}