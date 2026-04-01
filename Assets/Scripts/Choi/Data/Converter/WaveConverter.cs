#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class WaveConverter
{
    // 프리팹이 위치한 리소스 상대 경로 (Resources.Load용)
    private const string PREFAB_PATH = "Prefabs/CYC/Enemy";

    [MenuItem("Tools/웨이브 SO 베이킹")]
    public static void ConvertWave()
    {
        TextAsset csv = Resources.Load<TextAsset>("Data/WaveData");
        if (csv == null)
        {
            Debug.LogError("Resources/Data/WaveData.csv 파일을 찾을 수 없습니다.");
            return;
        }

        string folderPath = "Assets/Resources/Data/Waves2";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string[] lines = csv.text.Trim().Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] row = lines[i].Split(',');

            if (!int.TryParse(row[0].Trim(), out int waveNum)) continue;

            WaveData asset = ScriptableObject.CreateInstance<WaveData>();
            asset.waveName = $"Wave_{waveNum:D2}";
            asset.delayBeforeWave = 3.0f;

            float commonHp = GetFloat(row, 1);
            float bossHp = GetFloat(row, 2);
            int gold = (int)GetFloat(row, 3);

            // 보스 웨이브 판별 및 할당
            if (bossHp > 0 || GetFloat(row, 8) > 0)
            {
                asset.waveType = WaveType.Boss;
                asset.bossHp = bossHp;
                asset.bossGoldReward = gold;
                asset.bossPrefab = Resources.Load<Enemy>($"{PREFAB_PATH}/Boss");
            }
            else
            {
                asset.waveType = WaveType.Normal;
                // 각 컬럼의 숫자가 0보다 크면 해당 이름의 프리팹을 로드하여 리스트에 추가
                AddMonster(asset, "Normal", (int)GetFloat(row, 4), commonHp, gold);
                AddMonster(asset, "Speed", (int)GetFloat(row, 5), commonHp, gold);
                AddMonster(asset, "Branch", (int)GetFloat(row, 6), commonHp, gold);
                AddMonster(asset, "Stone", (int)GetFloat(row, 7), commonHp, gold);
            }

            string path = $"{folderPath}/Wave_{waveNum:D2}.asset";
            AssetDatabase.CreateAsset(asset, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan><b>[Wave Converter]</b> 웨이브 SO 변환 및 프리팹 연결 완료!</color>");
    }

    private static void AddMonster(WaveData data, string prefabName, int count, float hp, int gold)
    {
        if (count <= 0) return;

        // 지정된 경로에서 프리팹 로드
        Enemy prefab = Resources.Load<Enemy>($"{PREFAB_PATH}/{prefabName}");

        if (prefab == null)
        {
            Debug.LogWarning($"[WaveConverter] 프리팹을 찾을 수 없습니다: {PREFAB_PATH}/{prefabName}");
            return;
        }

        WaveMonsterInfo info = new WaveMonsterInfo
        {
            monsterPrefab = prefab,
            count = count,
            hpOverride = hp,
            goldReward = gold
        };

        data.monsterTypes.Add(info);
    }

    private static float GetFloat(string[] row, int index)
    {
        if (index >= row.Length || string.IsNullOrWhiteSpace(row[index])) return 0f;
        string cleanStr = row[index].Trim().Replace("\"", "").Replace(",", "");
        return float.TryParse(cleanStr, out float res) ? res : 0f;
    }
}
#endif