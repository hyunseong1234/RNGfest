using Dev.cheol.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class CSVDataManager : MonoBehaviour
    {
        private Dictionary<string, string[]> _towerTable = new();
        private Dictionary<string, string[]> _enemyTable = new();

        public void Init()
        {
            LoadTable("Data/TowerData", _towerTable);
            LoadTable("Data/EnemyData", _enemyTable);
            Debug.Log("모든 스탯 데이터 로드 완료");
        }

        private void LoadTable(string path, Dictionary<string, string[]> table)
        {
            table.Clear();
            TextAsset csvFile = Resources.Load<TextAsset>(path);
            if (csvFile == null) return;

            string[] lines = csvFile.text.Trim().Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] row = lines[i].Split(',');

                // TowerData.csv: Name(0), Rank(1), Damage(2), Speed(3), Range(4), Specials(5)
                // EnemyData.csv: Name(0), HP(1), Speed(2), Defense(3)
                string key = (path.Contains("Tower")) ? $"{row[0].Trim()}_{row[1].Trim()}" : row[0].Trim();
                table[key] = row;
            }
        }

        // --- 타워 스탯 설정 ---
        public void SetTowerStats(string name, int rank, BaseUnitStats stats)
        {
            string key = $"{name}_{rank}";
            if (!_towerTable.TryGetValue(key, out string[] row))
            {
                Debug.LogWarning($"[CSV] {key} 데이터를 찾을 수 없습니다. 시트를 확인하세요.");
                return;
            }

            // 2. 새로운 랭크 스탯을 적용하기 전, 기존 수정치 초기화 (선택 사항)
            // 랭크업 시 버프가 사라져야 한다면 아래 주석을 해제하세요.
            stats.Damage.ClearModifiers();
            stats.Speed.ClearModifiers();
            stats.Range.ClearModifiers();


            // 엑셀 순서: Name(0), Rank(1), Damage(2), Speed(3), Range(4), Specials(5)
            if (stats.Damage != null) stats.Damage.BaseValue = GetSafeFloat(row, 2);
            if (stats.Speed != null) stats.Speed.BaseValue = GetSafeFloat(row, 3);
            if (stats.Range != null) stats.Range.BaseValue = GetSafeFloat(row, 4);

            // SpecialValues (Stat 리스트) 처리
            if (row.Length > 5)
            {
                ApplySpecialStats(row[5], stats.SpecialValues);
            }
        }

        // --- 에너미 스탯 설정 ---
        public void SetEnemyStats(string name, BaseUnitStats stats)
        {
            if (!_enemyTable.TryGetValue(name, out string[] row)) return;

            // 엑셀 순서: Name(0), HP(1), Speed(2), Defense(3)
            if (stats.MaxHp != null)
            {
                stats.MaxHp.BaseValue = GetSafeFloat(row, 1);
                stats.CurrentHp = stats.MaxHp.Value; // Stat.Value 속성 사용
            }

            if (stats.Speed != null) stats.Speed.BaseValue = GetSafeFloat(row, 2);

            if (row.Length > 3 && stats.Defense != null)
            {
                stats.Defense.BaseValue = GetSafeFloat(row, 3);
            }
        }

        // SpecialValues 리스트의 각 Stat에 BaseValue 할당
        private void ApplySpecialStats(string raw, List<Stat> statList)
        {
            if (string.IsNullOrWhiteSpace(raw) || statList == null) return;

            string[] split = raw.Trim().Split(';');

            for (int i = 0; i < split.Length; i++)
            {
                // 리스트에 Stat 객체가 부족하면 새로 생성해서 넣어줌 (Null 방지)
                if (i >= statList.Count)
                {
                    statList.Add(new Stat());
                }

                if (float.TryParse(split[i].Trim(), out float res))
                {
                    statList[i].BaseValue = res;
                }
            }
        }

        private float GetSafeFloat(string[] row, int index)
        {
            if (index >= row.Length || string.IsNullOrWhiteSpace(row[index])) return 0f;
            string cleanStr = row[index].Trim().Replace("\"", "");
            return float.TryParse(cleanStr, out float res) ? res : 0f;
        }
    }
}