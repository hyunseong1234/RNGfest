#if UNITY_EDITOR
using Dev.jeon.Data;
using Dev.jeon.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace Dev.jeon.Editor.Importer
{
    /// <summary>
    /// CSV → AugmentData SO 생성
    /// Tools/Import Augment Data CSV
    /// 
    /// CSV 형식: Name,Description,Weight,AugmentType,TargetTower,Value
    /// </summary>
    public class AugmentDataImporter : EditorWindow
    {
        private string _csvPath = "Assets/Data/CSV/AugmentData.csv";
        private string _outputPath = "Assets/Data/Augments";

        [MenuItem("Tools/Import Augment Data CSV")]
        public static void ShowWindow()
        {
            GetWindow<AugmentDataImporter>("Augment Data Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Augment Data CSV Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("CSV 경로:");
            _csvPath = GUILayout.TextField(_csvPath);

            GUILayout.Label("SO 저장 경로:");
            _outputPath = GUILayout.TextField(_outputPath);

            GUILayout.Space(10);

            if (GUILayout.Button("AugmentData SO 생성", GUILayout.Height(40)))
                Import();
        }

        private void Import()
        {
            string[] lines = CsvParser.ReadLines(_csvPath);
            if (lines == null)
            {
                EditorUtility.DisplayDialog("오류", $"파일을 찾을 수 없습니다:\n{_csvPath}", "확인");
                return;
            }

            CsvParser.EnsureDirectory(_outputPath);

            int count = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 6) continue;

                string augName = cols[0].Trim();
                string desc = cols[1].Trim();
                int weight = CsvParser.ParseInt(cols[2], 50);
                string typeName = cols[3].Trim();
                string towerName = cols[4].Trim();
                float value = CsvParser.ParseFloat(cols[5]);

                if (!System.Enum.TryParse(typeName, out AugmentEffectType effectType))
                {
                    Debug.LogWarning($"[AugmentDataImporter] 알 수 없는 타입: {typeName} (행 {i + 1})");
                    continue;
                }

                if (!System.Enum.TryParse(towerName, out TowerType targetTower))
                    targetTower = TowerType.None;

                string assetPath = $"{_outputPath}/{augName}.asset";
                AugmentData augData = AssetDatabase.LoadAssetAtPath<AugmentData>(assetPath);

                if (augData == null)
                {
                    augData = ScriptableObject.CreateInstance<AugmentData>();
                    AssetDatabase.CreateAsset(augData, assetPath);
                }

                augData.augmentName = augName;
                augData.description = desc;
                augData.weight = weight;
                augData.effectType = effectType;
                augData.targetTowerType = targetTower;
                augData.value = value;

                EditorUtility.SetDirty(augData);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"AugmentData SO {count}개 완료!", "확인");
            Debug.Log($"[AugmentDataImporter] {count}개 증강 임포트 완료");
        }
    }
}
#endif