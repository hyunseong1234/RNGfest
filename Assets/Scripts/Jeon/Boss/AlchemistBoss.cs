using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class AlchemistBoss : BaseBoss
    {
        protected override void ApplySkillEffect()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var factory = ServiceLocator.Instance.GetService<FactoryManager>();

            if (main == null || pool == null || factory == null) return;

            // 1. 강제 합성 로직
            var activeTowers = main.SpawnTowers
                .Where(t => t != null && t.gameObject.activeSelf)
                .ToList();

            // 이름(PoolTag)과 랭크(Lank)가 같은 타워끼리 그룹화
            var mergeableGroups = activeTowers
                .GroupBy(t => new { t.PoolTag, t.Lank })
                .Where(g => g.Count() >= 2)
                .ToList();

            if (mergeableGroups.Count > 0)
            {
                var targetGroup = mergeableGroups[Random.Range(0, mergeableGroups.Count)].ToList();
                Tower target1 = targetGroup[0]; // 진화할 타워
                Tower target2 = targetGroup[1]; // 제물 타워

                Debug.Log($"[연금술사 보스] {target1.name}({target1.Lank}성) 강제 합성 실시!");

                // 제물을 풀에 반납하고, 타겟 타워의 랭크를 올림
                pool.ReturnPool(target2);
                target1.Lank++;

                var data = factory.GetTowerData(target1.PoolTag);
                if (data != null)
                {
                    target1.Setup(data, target1.Lank);
                }
            }
            else
            {
                Debug.Log("[연금술사 보스] 필드에 합성 가능한 타워가 없습니다.");
            }

            // 2. 공격이 끝났으니 1초 뒤에 체력을 회복하는 코루틴 실행
            StartCoroutine(DelayedHealRoutine());
        }

        private IEnumerator DelayedHealRoutine()
        {
            // 기획 조건: 공격 후 1초 대기
            yield return new WaitForSeconds(1.0f);

            // 최대 체력의 5% 계산 (_stat.MaxHp.Value 사용)
            float healAmount = _stat.MaxHp.Value * 0.05f;

            // [핵심 수정] _currentHp 대신 _stat.CurrentHp에 더해줍니다.
            _stat.CurrentHp += healAmount;

            // 최대 체력 초과 방지 (풀피 이상으로 회복 안 됨)
            if (_stat.CurrentHp > _stat.MaxHp.Value)
            {
                _stat.CurrentHp = _stat.MaxHp.Value;
            }

            Debug.Log($"[연금술사 보스] 체력 {healAmount} 회복! (현재 체력: {_stat.CurrentHp})");

            // TODO: 만약 보스 체력바 UI가 있다면 여기서 갱신해 주면 완벽합니다.
        }
    }
}