using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class IceBoss : BaseBoss
    {
        protected override void ApplySkillEffect()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main.SpawnTowers == null || main.SpawnTowers.Count == 0) return;

            // 이미 봉인된 타워 제외하고 찾기 (선택 사항)
            var availableTowers = main.SpawnTowers.FindAll(t => !t.IsSealed);

            if (availableTowers.Count > 0)
            {
                int rand = Random.Range(0, availableTowers.Count);
                availableTowers[rand].Seal();
            }
        }
    }
}
