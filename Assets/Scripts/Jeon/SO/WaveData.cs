using UnityEngine;
using System.Collections.Generic;

// 웨이브의 종류를 명확히 구분하기 위한 열거형(Enum)
public enum WaveType
{
    Normal, // 일반 몬스터 웨이브
    Boss    // 보스 웨이브
}

[System.Serializable]
public class WaveMonsterInfo
{
    [Header("소환할 몬스터 종류")]
    public Enemy monsterPrefab;
    [Header("소환할 마리 수")]
    public int count;
}

[CreateAssetMenu(fileName = "Wave_00", menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{
    // TODO : 일단 웨이브 
    [Header("웨이브 정보")]
    public string waveName;
    public WaveType waveType = WaveType.Normal;

    [Space(10)]
    [Header("웨이브 시작 전 대기 시간")]
    [Tooltip("이 웨이브가 시작되기 전에 몇 초를 기다릴지 설정합니다.")]
    public float delayBeforeWave = 3.0f;

    [Space(10)]
    [Header("일반 웨이브 설정")]
    public List<WaveMonsterInfo> monsterTypes = new List<WaveMonsterInfo>();

    [Space(10)]
    [Header("보스 웨이브 설정")]
    public Enemy bossPrefab;

    // 총 몬스터 수를 수동으로 입력하지 않고 자동으로 계산해주는 프로퍼티
    public int TotalMonsterCount
    {
        get
        {
            if (waveType == WaveType.Boss)
            {
                return 1; // 보스 웨이브는 무조건 1마리
            }

            int total = 0;
            foreach (var info in monsterTypes)
            {
                total += info.count;
            }
            return total;
        }
    }
}