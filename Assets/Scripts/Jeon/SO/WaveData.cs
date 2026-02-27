using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveMonsterInfo
{
    [Header("소환할 몬스터 종류 및 마리 수")]
    // 팩토리가 생성해둔 프리팹(일반, 이속 빠른 놈 등)을 직접 연결합니다.
    public Enemy monsterPrefab;
    public int count; // 이 몬스터를 몇 마리 뽑을 것인가
}

[CreateAssetMenu(fileName = "Wave_00", menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("A. 몬스터 웨이브 (예: 1 Wave)")]
    public string waveName;

    [Header("B. 몬스터 총 수")]
    public int totalMonsterCount;

    [Header("C. 몬스터 종류 선택 (최대 4종류 권장)")]
    // 엑셀의 '일반=7, 이속=3' 등을 리스트로 추가해서 만듭니다.
    public List<WaveMonsterInfo> monsterTypes = new List<WaveMonsterInfo>();

    [Header("D. 보스 몬스터")]
    public bool hasBoss; // 엑셀의 'X'면 false, 보스가 있으면 true
    public Enemy bossPrefab; // hasBoss가 true일 때 소환할 보스 프리팹
}