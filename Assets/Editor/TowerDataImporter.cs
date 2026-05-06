#if UNITY_EDITOR
using Dev.jeon.Data;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dev.jeon.Editor
{
    /// <summary>
    /// CSV → TowerData / AugmentData ScriptableObject 자동 생성 에디터 툴
    /// 사용법:
    /// 1. 엑셀/구글시트에서 CSV로 저장 (UTF-8)
    /// 2. Assets/Data/CSV/ 폴더에 파일 넣기
    /// 3. Unity 상단 메뉴 → Tools → Import Tower Data CSV 클릭
    /// 
    /// [AugmentData CSV 형식]
    /// Name,Description,Weight,AugmentType,TargetTower,Value
    /// FireDamageUp,화염 공격력 30% 증가,80,StatModifier_Damage,Fire,0.3
    /// </summary>
    public class TowerDataImporter : EditorWindow
    {
        private string _towerCsvPath = "Assets/Data/CSV/TowerData.csv";
        private string _augmentCsvPath = "Assets/Data/CSV/AugmentData.csv";
        private string _towerOutputPath = "Assets/Data/Towers";
        private string _augmentOutputPath = "Assets/Data/Augments";

        [MenuItem("Tools/Import Tower Data CSV")]
        public static void ShowWindow()
        {
            GetWindow<TowerDataImporter>("Tower Data Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tower Data CSV Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Tower CSV 경로:");
            _towerCsvPath = GUILayout.TextField(_towerCsvPath);

            GUILayout.Label("Augment CSV 경로:");
            _augmentCsvPath = GUILayout.TextField(_augmentCsvPath);

            GUILayout.Space(5);

            GUILayout.Label("Tower SO 저장 경로:");
            _towerOutputPath = GUILayout.TextField(_towerOutputPath);

            GUILayout.Label("Augment SO 저장 경로:");
            _augmentOutputPath = GUILayout.TextField(_augmentOutputPath);

            GUILayout.Space(10);

            if (GUILayout.Button("CSV에서 TowerData SO 생성", GUILayout.Height(40)))
                ImportTowerData();

            GUILayout.Space(5);

            if (GUILayout.Button("CSV에서 AugmentData SO 생성", GUILayout.Height(40)))
                ImportAugmentData();

            GUILayout.Space(5);

            if (GUILayout.Button("전체 Import (Tower + Augment)", GUILayout.Height(40)))
            {
                ImportTowerData();
                ImportAugmentData();
            }
        }

        /// <summary>
        /// TowerData CSV 파싱
        /// Name,Rank,Attack,Speed,Range,Value1,Value2,Value3
        /// </summary>
        private void ImportTowerData()
        {
            if (!File.Exists(_towerCsvPath))
            {
                EditorUtility.DisplayDialog("오류", $"Tower CSV 파일을 찾을 수 없습니다:\n{_towerCsvPath}", "확인");
                return;
            }

            if (!Directory.Exists(_towerOutputPath))
                Directory.CreateDirectory(_towerOutputPath);

            string[] lines = File.ReadAllLines(_towerCsvPath, System.Text.Encoding.UTF8);
            var towerDict = new Dictionary<string, TowerData>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 5) continue;

                string towerName = cols[0].Trim();
                if (!int.TryParse(cols[1].Trim(), out int rank)) continue;

                float attack = ParseFloat(cols[2]);
                float speed = ParseFloat(cols[3]);
                float range = ParseFloat(cols[4]);

                List<float> specials = new List<float>();
                for (int v = 5; v < cols.Length; v++)
                {
                    string val = cols[v].Trim();
                    if (!string.IsNullOrEmpty(val) && float.TryParse(val, out float sv))
                        specials.Add(sv);
                }

                if (!towerDict.ContainsKey(towerName))
                {
                    string assetPath = $"{_towerOutputPath}/{towerName}.asset";
                    TowerData existing = AssetDatabase.LoadAssetAtPath<TowerData>(assetPath);

                    if (existing == null)
                    {
                        existing = ScriptableObject.CreateInstance<TowerData>();
                        existing.towerName = towerName;
                        AssetDatabase.CreateAsset(existing, assetPath);
                    }
                    else
                    {
                        existing.stats.Clear();
                    }

                    towerDict[towerName] = existing;
                }

                towerDict[towerName].stats.Add(new TowerStat
                {
                    rank = rank,
                    attack = attack,
                    speed = speed,
                    range = range,
                    specialValues = specials
                });

                EditorUtility.SetDirty(towerDict[towerName]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"TowerData SO {towerDict.Count}개 생성/갱신 완료!", "확인");
            Debug.Log($"[TowerDataImporter] {towerDict.Count}개 타워 데이터 임포트 완료");
        }

        /// <summary>
        /// AugmentData CSV 파싱
        /// Name,Description,Weight,AugmentType,TargetTower,Value
        /// </summary>
        private void ImportAugmentData()
        {
            if (!File.Exists(_augmentCsvPath))
            {
                EditorUtility.DisplayDialog("오류", $"Augment CSV 파일을 찾을 수 없습니다:\n{_augmentCsvPath}", "확인");
                return;
            }

            if (!Directory.Exists(_augmentOutputPath))
                Directory.CreateDirectory(_augmentOutputPath);

            string[] lines = File.ReadAllLines(_augmentCsvPath, System.Text.Encoding.UTF8);
            int count = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 6) continue;

                string augName = cols[0].Trim();
                string desc = cols[1].Trim();
                int weight = int.TryParse(cols[2].Trim(), out int w) ? w : 50;
                string typeName = cols[3].Trim();
                string towerName = cols[4].Trim();
                float value = ParseFloat(cols[5]);

                // AugmentEffectType 파싱
                if (!System.Enum.TryParse(typeName, out AugmentEffectType effectType))
                {
                    Debug.LogWarning($"[TowerDataImporter] 알 수 없는 AugmentEffectType: {typeName} (행 {i + 1})");
                    continue;
                }

                // TowerType 파싱 (None이면 타워 무관)
                if (!System.Enum.TryParse(towerName, out TowerType targetTower))
                    targetTower = TowerType.None;

                string assetPath = $"{_augmentOutputPath}/{augName}.asset";
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
            EditorUtility.DisplayDialog("완료", $"AugmentData SO {count}개 생성/갱신 완료!", "확인");
            Debug.Log($"[TowerDataImporter] {count}개 증강 데이터 임포트 완료");
        }

        // "1,800" 같은 숫자 파싱
        private float ParseFloat(string raw)
        {
            string cleaned = raw.Trim().Replace(",", "");
            return float.TryParse(cleaned, out float result) ? result : 0f;
        }
    }
}
#endif