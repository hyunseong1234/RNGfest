using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet;
using Dev.jeon.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class ShamanBoss : BaseBoss
    {
        [Header("Curse Skill Settings")]
        [SerializeField] private SkillBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 1.5f; // 여기서 경고 해결!

        [Header("Dash Settings")]
        [SerializeField] private float _dashDistance = 1.0f;
        [SerializeField] private float _dashDuration = 0.2f;

        private bool _isCurseUsed = false;

        protected override void ApplySkillEffect()
        {
            // 첫 번째 스킬은 무조건 저주 발사
            if (!_isCurseUsed)
            {
                CastCurse();
                _isCurseUsed = true;
            }
            // 그 다음부터는 웨이포인트를 향해 대쉬
            else
            {
                StartCoroutine(SmoothDashRoutine());
            }
        }

        private void CastCurse()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var towers = main.SpawnTowers;

            if (towers == null || towers.Count == 0) return;

            // 1. 타워 리스트를 랜덤하게 섞고 절반을 선택
            var randomTowers = towers.OrderBy(x => Random.value).ToList();
            int targetCount = Mathf.Max(1, randomTowers.Count / 2);

            for (int i = 0; i < targetCount; i++)
            {
                Tower target = randomTowers[i];

                // 2. 풀에서 스킬 탄 가져오기
                var bullet = pool.GetFromPool<SkillBullet>(_bulletPrefab);
                if (bullet != null)
                {
                    bullet.transform.position = transform.position + Vector3.up * 2f;

                    // [핵심] 여기서 _bulletSpeed를 사용하여 경고를 해결합니다.
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.SHAMAN);
                }
            }

            Debug.Log($"[주술사 보스] 등장 첫 스킬! 타워 {targetCount}개에 포물선 저주탄 발사!");
        }

        private IEnumerator SmoothDashRoutine()
        {
            if (Target == null) yield break;

            Vector3 startPos = transform.position;
            // Enemy.cs에 정의된 Target(다음 깃발) 방향으로 대쉬
            Vector3 endPos = Vector3.MoveTowards(startPos, Target.position, _dashDistance);

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _dashDuration;

                // deltaTime을 활용한 부드러운 선형 보간 이동
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            Debug.Log("[주술사 보스] 4초 쿨타임 후 1칸 대쉬 완료!");
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            // 풀로 돌아갈 때(죽거나 골인 시) 다시 첫 스킬을 쓸 수 있게 초기화
            _isCurseUsed = false;
        }
    }
}