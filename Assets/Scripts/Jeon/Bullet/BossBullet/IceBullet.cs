using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class IceBullet : BossBullet
    {
        // 아이스만의 특수 효과: 타워 봉인(Seal) 실행
        protected override void ApplySkillEffect(Tower targetTower)
        {
            // 타워 스크립트의 Seal 함수 호출
            targetTower.Seal(_effectPrefab);
        }

    }
}