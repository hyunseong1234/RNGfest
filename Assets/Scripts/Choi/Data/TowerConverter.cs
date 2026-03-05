#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TowerConverter
{
    // CSV 파일이 위치한 경로 (수정 가능)
    private const string CSV_PATH = "Assets/Resources/Data/TowerData.csv";
    private const string SAVE_PATH = "Assets/Resources/Data/Towers";

    [MenuItem("Tools/Convert Tower CSV to SO (Direct)")]
    public static void ConvertDirect()
    {
        // 1. 파일 존재 여부 확인 및 읽기
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"[TowerConverter] CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }

        // 파일 공유 위반 방지(FileShare.ReadWrite) 옵션 적용
        List<string> lineList = new List<string>();
        using (var fs = new FileStream(CSV_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(fs))
        {
            while (!reader.EndOfStream) lineList.Add(reader.ReadLine());
        }

        string[] lines = lineList.ToArray();
        Dictionary<string, List<TowerStat>> towerGroups = new Dictionary<string, List<TowerStat>>();

        // 2. 파싱 로직
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] row = line.Split(',');
            if (row.Length < 5) continue;

            string towerName = row[0].Trim();
            try
            {
                TowerStat stat = new TowerStat
                {
                    rank = int.Parse(Clean(row[1])),
                    attack = float.Parse(Clean(row[2]), CultureInfo.InvariantCulture),
                    speed = float.Parse(Clean(row[3]), CultureInfo.InvariantCulture),
                    range = float.Parse(Clean(row[4]), CultureInfo.InvariantCulture)
                };

                for (int j = 5; j < row.Length; j++)
                {
                    if (!string.IsNullOrWhiteSpace(row[j]))
                    {
                        stat.specialValues.Add(float.Parse(Clean(row[j]), CultureInfo.InvariantCulture));
                    }
                }

                if (!towerGroups.ContainsKey(towerName))
                    towerGroups[towerName] = new List<TowerStat>();

                towerGroups[towerName].Add(stat);
            }
            catch
            {
                // 에러 발생 시 로그 출력 (41행 등 문제 지점 확인용)
                Debug.LogError($"[파싱 실패] {i + 1}행 데이터 오류: {line}");
            }
        }

        // 3. SO 저장 및 생성
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
        Debug.Log($"<color=cyan><b>[Tower Converter]</b></color> {CSV_PATH} 데이터 변환 완료!");
    }

    private static string Clean(string input) => input.Trim().Replace("\"", "").Replace(",", "");
}
#endif