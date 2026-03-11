using Dev.cheol.Model;

namespace Dev.jeon.Bullet
{
    public class IceBullet : BossBullet
    {
        protected override void ApplySkillEffect(Tower targetTower)
        {
            targetTower.Seal(_effectPrefab); // Ice 전용 이펙트와 함께 봉인
        }
    }
}