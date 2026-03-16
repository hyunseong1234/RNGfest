using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    public class IceBossBullet : BossBullet
    {
        protected override void ApplySkillEffect(Tower targetTower)
        {
            // 1. 타워가 봉인 상태가 아니라면
            if (!targetTower.IsSealed)
            {
                // 2. 타워의 Seal 함수를 호출하여 상태이상 프리팹을 넘겨줍니다.
                // 그러면 타워가 스스로 이펙트를 생성해서 자기 자식으로 붙이고(_currentEffect) 관리합니다.
                targetTower.Seal(_effectPrefab);
            }
        }
    }
}