using UnityEngine;

namespace Dev.jeon.Data
{
    // 증강 타입
    public enum AugmentType
    {
        TowerBuff,   // 특정 타워 강화
        Economy,     // 경제 관련
        Ultimate     // 궁극기 해금
    }

    // 증강이 강화할 스탯 종류
    public enum AugmentStatType
    {
        Damage,      // 공격력
        Speed,       // 공격속도
        Range,       // 사거리
        SpecialValue // Value1 (슬로우%, 독 데미지 등 타워 고유 수치)
    }

    [CreateAssetMenu(fileName = "NewAugment", menuName = "Data/AugmentData")]
    public class AugmentData : ScriptableObject
    {
        [Header("기본 정보")]
        public string augmentName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("등장 가중치 (높을수록 자주 나옴)")]
        public int weight = 50;

        [Header("증강 타입")]
        public AugmentType augmentType;

        [Header("타워 버프 설정 (TowerBuff일 때만 사용)")]
        public TowerType targetTowerType;   // 어떤 타워에 적용할지
        public AugmentStatType statType;    // 어떤 스탯을 강화할지
        public float value;                 // 강화 수치 (0.3 = 30% 증가)

        [Header("경제 설정 (Economy일 때만 사용)")]
        public int bonusGoldPerKill;        // 처치 시 추가 골드
        public float interestRate;          // 이자율 (0.1 = 10%)

        [Header("궁극기 설정 (Ultimate일 때만 사용)")]
        public TowerType ultimateTowerType; // 어떤 타워의 궁극기를 해금할지
    }
}