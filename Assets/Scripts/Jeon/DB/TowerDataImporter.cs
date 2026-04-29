#if UNITY_EDITOR
using Dev.cheol.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dev.cheol.Editor
{
    /// <summary>
    /// CSV → TowerData / AugmentData ScriptableObject 자동 생성 에디터 툴
    /// 사용법:
    /// 1. 엑셀에서 CSV로 저장 (UTF-8)
    /// 2. Assets/Data/CSV/ 폴더에 파일 넣기
    /// 3. Unity 상단 메뉴 → Tools → Import Tower Data CSV 클릭
    /// </summary>
    public class TowerDataImporter : EditorWindow
    {
        private string _csvPath = "Assets/Data/CSV/TowerData.csv";
        private string _outputPath = "Assets/Data/Towers";

        [MenuItem("Tools/Import Tower Data CSV")]
        public static void ShowWindow()
        {
            GetWindow<TowerDataImporter>("Tower Data Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tower Data CSV Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("CSV 파일 경로:");
            _csvPath = GUILayout.TextField(_csvPath);

            GUILayout.Label("SO 저장 경로:");
            _outputPath = GUILayout.TextField(_outputPath);

            GUILayout.Space(10);

            if (GUILayout.Button("CSV에서 TowerData SO 생성", GUILayout.Height(40)))
                ImportTowerData();

            GUILayout.Space(5);

            if (GUILayout.Button("CSV에서 AugmentData SO 생성", GUILayout.Height(40)))
                ImportAugmentData();
        }

        /// <summary>
        /// CSV 형식:
        /// Name,Rank,Attack,Speed,Range,Value1,Value2,Value3
        /// 01FireTower,1,8,1,5,1.5,,
        /// </summary>
        private void ImportTowerData()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("오류", $"CSV 파일을 찾을 수 없습니다:\n{_csvPath}", "확인");
                return;
            }

            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);

            string[] lines = File.ReadAllLines(_csvPath, System.Text.Encoding.UTF8);
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
                float speed  = ParseFloat(cols[3]);
                float range  = ParseFloat(cols[4]);

                List<float> specials = new List<float>();
                for (int v = 5; v < cols.Length; v++)
                {
                    string val = cols[v].Trim();
                    if (!string.IsNullOrEmpty(val) && float.TryParse(val, out float sv))
                        specials.Add(sv);
                }

                if (!towerDict.ContainsKey(towerName))
                {
                    string assetPath = $"{_outputPath}/{towerName}.asset";
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
        /// Augment CSV 형식:
        /// AugmentName,Description,Weight,AugmentType,TargetTower,StatType,Value,BonusGold,InterestRate,UltimateTower
        /// FireDamageUp,파이어 타워 공격력 +30%,80,TowerBuff,Fire,Damage,0.3,0,0,None
        /// </summary>
        private void ImportAugmentData()
        {
            string augmentCsvPath   = _csvPath.Replace("TowerData", "AugmentData");
            string augmentOutputPath = _outputPath.Replace("Towers", "Augments");

            if (!File.Exists(augmentCsvPath))
            {
                EditorUtility.DisplayDialog("오류",
                    $"Augment CSV 파일을 찾을 수 없습니다:\n{augmentCsvPath}\n\n먼저 AugmentData.csv를 만들어주세요.", "확인");
                return;
            }

            if (!Directory.Exists(augmentOutputPath))
                Directory.CreateDirectory(augmentOutputPath);

            string[] lines = File.ReadAllLines(augmentCsvPath, System.Text.Encoding.UTF8);
            int count = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 6) continue;

                string augName = cols[0].Trim();
                string desc    = cols[1].Trim();
                int weight     = int.TryParse(cols[2].Trim(), out int w) ? w : 50;

                if (!System.Enum.TryParse(cols[3].Trim(), out AugmentType augType)) continue;
                if (!System.Enum.TryParse(cols[4].Trim(), out TowerType targetTower)) targetTower = TowerType.None;
                if (!System.Enum.TryParse(cols[5].Trim(), out AugmentStatType statType)) statType = AugmentStatType.Damage;

                float value        = cols.Length > 6 ? ParseFloat(cols[6]) : 0;
                int bonusGold      = cols.Length > 7 && int.TryParse(cols[7].Trim(), out int bg) ? bg : 0;
                float interestRate = cols.Length > 8 ? ParseFloat(cols[8]) : 0;

                TowerType ultimateType = TowerType.None;
                if (cols.Length > 9) System.Enum.TryParse(cols[9].Trim(), out ultimateType);

                string assetPath = $"{augmentOutputPath}/{augName}.asset";
                AugmentData augData = AssetDatabase.LoadAssetAtPath<AugmentData>(assetPath);

                if (augData == null)
                {
                    augData = ScriptableObject.CreateInstance<AugmentData>();
                    AssetDatabase.CreateAsset(augData, assetPath);
                }

                augData.augmentName       = augName;
                augData.description       = desc;
                augData.weight            = weight;
                augData.augmentType       = augType;
                augData.targetTowerType   = targetTower;
                augData.statType          = statType;
                augData.value             = value;
                augData.bonusGoldPerKill  = bonusGold;
                augData.interestRate      = interestRate;
                augData.ultimateTowerType = ultimateType;

                EditorUtility.SetDirty(augData);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"AugmentData SO {count}개 생성/갱신 완료!", "확인");
            Debug.Log($"[TowerDataImporter] {count}개 증강 데이터 임포트 완료");
        }

        // "1,800" 같은 숫자도 파싱 가능
        private float ParseFloat(string raw)
        {
            string cleaned = raw.Trim().Replace(",", "");
            return float.TryParse(cleaned, out float result) ? result : 0f;
        }
    }
}
#endif
