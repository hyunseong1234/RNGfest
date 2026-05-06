namespace Dev.jeon.Model
{
    /// <summary>
    /// 타워 고유 특수 수치를 증강으로 강화할 때 사용하는 인터페이스
    /// 각 Bullet에서 구현
    /// FireBullet   → _splashRadius 증가
    /// SlowBullet   → _slowAmount 증가
    /// PoisonBullet → _poisonDamage 증가
    /// AdeleBullet  → _attackSpeed 증가
    /// ElectricityBullet → _maxTargets 증가
    /// </summary>
    public interface IAbilityBoost
    {
        void ApplyAbilityBoost(float value);
    }
}