#if UNITY_EDITOR
using Dev.jeon.Data;
using UnityEditor;
using UnityEngine;

namespace Dev.jeon.Editor.Importer
{
    /// <summary>
    /// AugmentData SO에 아이콘 자동 연결
    /// Tools/Assign Augment Icons
    /// 
    /// effectType + targetTowerType 기반으로 아이콘 자동 매핑
    /// </summary>
    public class AugmentIconAssigner : EditorWindow
    {
        private string _augmentPath = "Assets/Data/Augments";
        private string _iconPath = "Assets/Texture/AugmentIcons";

        [MenuItem("Tools/Assign Augment Icons")]
        public static void ShowWindow()
        {
            GetWindow<AugmentIconAssigner>("Augment Icon Assigner");
        }

        private void OnGUI()
        {
            GUILayout.Label("Augment Icon Assigner", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("AugmentData SO 경로:");
            _augmentPath = GUILayout.TextField(_augmentPath);

            GUILayout.Label("아이콘 폴더 경로:");
            _iconPath = GUILayout.TextField(_iconPath);

            GUILayout.Space(10);

            if (GUILayout.Button("아이콘 자동 연결", GUILayout.Height(40)))
                Assign();
        }

        private void Assign()
        {
            string[] guids = AssetDatabase.FindAssets("t:AugmentData", new[] { _augmentPath });
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", $"AugmentData SO 없음:\n{_augmentPath}", "확인");
                return;
            }

            int success = 0;
            int fail = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AugmentData data = AssetDatabase.LoadAssetAtPath<AugmentData>(path);
                if (data == null) continue;

                string iconName = GetIconName(data);
                Sprite icon = LoadIcon(iconName);

                if (icon != null)
                {
                    data.icon = icon;
                    EditorUtility.SetDirty(data);
                    success++;
                }
                else
                {
                    Debug.LogWarning($"[AugmentIconAssigner] 아이콘 없음: {iconName} ({data.augmentName})");
                    fail++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"성공: {success}개\n실패: {fail}개", "확인");
            Debug.Log($"[AugmentIconAssigner] 성공: {success}, 실패: {fail}");
        }

        private string GetIconName(AugmentData data)
        {
            switch (data.effectType)
            {
                case AugmentEffectType.Economy_Gold: return "Gold";
                case AugmentEffectType.Economy_Interest: return "Interest";
                case AugmentEffectType.Ultimate:
                case AugmentEffectType.Ultimate_SlowZone: return "Ultimate";
                case AugmentEffectType.StatModifier_Damage: return "Attack";
                case AugmentEffectType.StatModifier_Speed: return "Speed";
                case AugmentEffectType.AbilityBoost: return GetAbilityIcon(data.targetTowerType);
                default: return "Attack";
            }
        }

        private string GetAbilityIcon(TowerType type)
        {
            switch (type)
            {
                case TowerType.Fire: return "Splash";
                case TowerType.Slow: return "Slow";
                case TowerType.Poison: return "Poison";
                case TowerType.Electric: return "Chain";
                case TowerType.Marking: return "Marking";
                case TowerType.Buff: return "Buff";
                case TowerType.Growth: return "Growth";
                case TowerType.Stationary: return "Stationary";
                case TowerType.Archer: return "Archer";
                case TowerType.Speed: return "Speed";
                case TowerType.Melee: return "Melee";  
                case TowerType.Adel: return "Adele";  
                default: return "Attack";
            }
        }
        private Sprite LoadIcon(string iconName)
        {
            // PNG 시도
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{_iconPath}/{iconName}.png");
            if (sprite != null) return sprite;

            // BMP 시도
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{_iconPath}/{iconName}.bmp");
            if (sprite != null) return sprite;

            // SVG 시도
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{_iconPath}/{iconName}.svg");


        }
    }
}
#endif