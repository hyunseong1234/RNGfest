#if UNITY_EDITOR
using Dev.jeon.Data;
using Dev.jeon.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dev.jeon.Editor.Importer
{
    /// <summary>
    /// CSV → TowerData SO 생성
    /// Tools/Import Tower Data CSV
    /// 
    /// CSV 형식: Name,Rank,Attack,Speed,Range,Value1,Value2,Value3
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

            GUILayout.Label("CSV 경로:");
            _csvPath = GUILayout.TextField(_csvPath);

            GUILayout.Label("SO 저장 경로:");
            _outputPath = GUILayout.TextField(_outputPath);

            GUILayout.Space(10);

            if (GUILayout.Button("TowerData SO 생성", GUILayout.Height(40)))
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

            var towerDict = new Dictionary<string, TowerData>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 5) continue;

                string towerName = cols[0].Trim();
                if (!int.TryParse(cols[1].Trim(), out int rank)) continue;

                float attack = CsvParser.ParseFloat(cols[2]);
                float speed = CsvParser.ParseFloat(cols[3]);
                float range = CsvParser.ParseFloat(cols[4]);

                var specials = new List<float>();
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
            EditorUtility.DisplayDialog("완료", $"TowerData SO {towerDict.Count}개 완료!", "확인");
            Debug.Log($"[TowerDataImporter] {towerDict.Count}개 타워 임포트 완료");
        }
    }
}
#endif