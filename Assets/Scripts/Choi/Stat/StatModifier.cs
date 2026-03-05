namespace Dev.cheol.Stats
{
    public enum StatModType
    {
        Flat = 100,      // 고정 수치 추가 (예: 공격력 +10)
        Percent = 200,   // 퍼센트 추가 (예: 공격력 +10% -> 0.1f)
    }
    public class StatModifier
    {
        public readonly float Value;    // 수정될 값
        public readonly StatModType Type; // 연산 방식
        public readonly object Source;   // 이 수치를 준 대상 (예: SlowBuff, PowerItem 등)

        public StatModifier(float value, StatModType type, object source)
        {
            Value = value;
            Type = type;
            Source = source;
        }

        // 소스 없이 생성하는 경우를 위한 생성자 오버로딩
        public StatModifier(float value, StatModType type) : this(value, type, null) { }
    }
}