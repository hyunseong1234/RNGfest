#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TowerConverter
{
    private const string CSV_PATH = "Assets/Resources/Data/TowerData.csv";
    private const string SAVE_PATH = "Assets/Resources/Data/Towers";

    [MenuItem("Tools/Convert Tower CSV to SO (Safe Mode)")]
    public static void ConvertDirect()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"[TowerConverter] CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }

        string[] lines = File.ReadAllLines(CSV_PATH);
        Dictionary<string, List<TowerStat>> towerGroups = new Dictionary<string, List<TowerStat>>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 쉼표와 탭 모두 대응 가능하도록 분리
            string[] row = line.Split(new char[] { ',', '\t' });
            if (row.Length < 2) continue;

            string towerName = row[0].Trim();

            // 헤더(Name, Rank 등)가 포함된 행은 자동으로 건너뜀
            if (!int.TryParse(Clean(row[1]), out _)) continue;

            try
            {
                TowerStat stat = new TowerStat
                {
                    // 숫자로 못 바꾸면 그냥 0으로 세팅해서 에러 방지
                    rank = ParseIntSafe(row.ElementAtOrDefault(1)),
                    attack = ParseFloatSafe(row.ElementAtOrDefault(2)),
                    speed = ParseFloatSafe(row.ElementAtOrDefault(3)),
                    range = ParseFloatSafe(row.ElementAtOrDefault(4)),
                    specialValues = new List<float>()
                };

                // 효과 수치들도 안전하게 파싱 (0이면 기입 안 함)
                for (int j = 5; j < row.Length; j++)
                {
                    float val = ParseFloatSafe(row[j]);
                    if (val != 0) stat.specialValues.Add(val);
                }

                if (!towerGroups.ContainsKey(towerName))
                    towerGroups[towerName] = new List<TowerStat>();

                towerGroups[towerName].Add(stat);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[{i + 1}행 건너뜀] 데이터 형식이 이상함: {line} ({e.Message})");
            }
        }

        if (!Directory.Exists(SAVE_PATH)) Directory.CreateDirectory(SAVE_PATH);

        foreach (var group in towerGroups)
        {
            string assetPath = $"{SAVE_PATH}/{group.Key}.asset";
            TowerData asset = AssetDatabase.LoadAssetAtPath<TowerData>(assetPath);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TowerData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.towerName = group.Key;
            asset.stats = group.Value.OrderBy(s => s.rank).ToList();
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=green><b>[변환 완료]</b></color> {towerGroups.Count}개의 타워 SO 생성/갱신됨.");
    }

    // 안전한 파싱용 헬퍼 함수들
    private static int ParseIntSafe(string s) => int.TryParse(Clean(s), out int result) ? result : 0;
    private static float ParseFloatSafe(string s) => float.TryParse(Clean(s), NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : 0f;
    private static string Clean(string input) => input?.Trim().Replace("\"", "").Replace(",", "") ?? "";
}
#endif