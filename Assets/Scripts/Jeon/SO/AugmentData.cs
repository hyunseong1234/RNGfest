using UnityEngine;

namespace Dev.jeon.Data
{
    /// <summary>
    /// CSV AugmentType 컬럼과 1:1 대응
    /// </summary>
    public enum AugmentEffectType
    {
        StatModifier_Damage,    // 공격력 % 증가
        StatModifier_Speed,     // 공격속도 % 증가
        AbilityBoost,           // 타워 고유 특수 수치 증가 (스플래시, 슬로우량, 독뎀, 연쇄횟수 등)
        Economy_Gold,           // 처치당 골드 추가
        Economy_Interest,       // 이자 시스템
        Ultimate,               // 타워 궁극기 해금
        Ultimate_SlowZone,      // 슬로우존 해금 (타워 무관)
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

        [Header("효과 타입 (CSV AugmentType과 동일)")]
        public AugmentEffectType effectType;

        [Header("대상 타워 (None이면 전체 or 타워 무관)")]
        public TowerType targetTowerType;

        [Header("효과 수치")]
        public float value;
    }
}