using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ShamanBullet : BossBullet
    {
        [Header("Shaman Custom Settings")]
        [SerializeField] private float _destroyDelay = 1.0f; // 강등/파괴 대기 시간
        [SerializeField] private BaseObject _destroyEffectPrefab; // 1성 타워 파괴 연출

        // 샤먼만의 특수 효과: 타워 강등 실행
        protected override void ApplySkillEffect(Tower targetTower)
        {
            // 타워 스크립트의 DowngradEffect 함수 호출
            targetTower.DowngradEffect(_effectPrefab, _destroyEffectPrefab, _destroyDelay);
        }

        // 부모의 기본 피격 이펙트(펑 터지는 등)를 무시하기 위해 오버라이드
        protected override void SpawnHitEffect(Vector3 pos)
        {
            // 저주 탄환은 부모의 공통 이펙트를 생성하지 않고 비워둡니다.
            // 만약 공통 이펙트가 필요하다면 base.SpawnHitEffect(pos); 를 호출하세요.
        }
    }
}