
using System;
using System.Collections.Generic;

[Serializable]
public class UserGameData
{
    public int _gold = 0;
    public int _jewel = 0;
    public string _userNickName;
    public List<TowerGameData> _towers;
    public bool _isDeleted = false;
    public string _lastLoginId = "";

    public List<TowerPreset> _towerSlots = new List<TowerPreset>();
    public int _currentSlot; //현재 슬롯 정보

    /// <summary>
    /// 데이터셋 
    /// </summary>
    /// <param name="sessionKey"></param>
    public void SetDefaultValues(string sessionKey)
    {
        _gold = 0;
        _jewel = 0;
        _currentSlot = 0;
        _isDeleted = false;
        _lastLoginId = sessionKey;

        // 보유 타워 초기화
        _towers = new List<TowerGameData>
        {
            new TowerGameData(TowerType.Fire, 1, 0),
            new TowerGameData(TowerType.Archer, 1, 0),
            new TowerGameData(TowerType.Speed, 1, 0),
            new TowerGameData(TowerType.Electric, 1, 0),
            new TowerGameData(TowerType.Poison, 1, 0)
        };

        // 프리셋 초기화 (4개 프리셋 생성, 각 덱마다 초기 타워 5개 세팅)
        _towerSlots = new List<TowerPreset>();
        for (int i = 0; i < 4; i++)
        {
            TowerPreset newSlot = new TowerPreset();

            if (i == 0) // TODO : 첫 번째 덱만 기본 타워 세팅 = 현재는 다 None으로 처리하고 듀토리얼 만들때 마춰서 세팅예정
            {
                newSlot.slotTowers = new List<TowerType>
                {
                    TowerType.None, TowerType.None, TowerType.None, TowerType.None, TowerType.None
                };
            }
            // 초기 덱 구성 보유 중인 기본 타워들로 세팅
            newSlot.slotTowers = new List<TowerType>
            {
                TowerType.None, TowerType.None, TowerType.None, TowerType.None, TowerType.None
            };
            _towerSlots.Add(newSlot);
        }
    }

    /// <summary>
    /// 가챠 결과로 받은 타워 리스트를 데이터에 반영합니다.
    /// 중복이면 경험치 +5, 없으면 신규 추가!
    /// </summary>
    public void AddGachaResults(List<TowerType> results)
    {
        foreach (TowerType resultId in results)
        {
            if (resultId == TowerType.None) continue;

            // 이미 보유 중인 타워인지 확인
            TowerGameData existingTower = _towers.Find(t => t._id == resultId);

            if (existingTower != null)
            {
                // 2. 중복인 경우: 경험치 +5
                existingTower._currentExp += 5;
                UnityEngine.Debug.Log($"{resultId} 중복 획득! 경험치 +5 (현재: {existingTower._currentExp})");
            }
            else
            {
                //없는 타워인 경우: 리스트에 새로 추가 (1레벨, 경험치 0)
                _towers.Add(new TowerGameData(resultId, 1, 0));
                UnityEngine.Debug.Log($"{resultId} 신규 획득! 리스트에 추가되었습니다.");
            }
        }
    }
}

[System.Serializable]
public class TowerPreset
{
    public List<TowerType> slotTowers = new List<TowerType>();
}
/// <summary>
/// 타워 종류 추가될때마다 여기다 이넘 추가해야됩니다.
/// </summary>
public enum TowerType
{
    None = 0,
    Fire = 1,
    Slow = 2,
    Archer = 3,
    Speed = 4,
    Electric = 5,
    Poison = 6,
    Stationary = 7,
    Marking = 8,
    Melee = 9,
    Buff = 10,
    Growth = 11,
    Adel = 12,


    Max = 9999
}

[Serializable]
public class TowerGameData
{
    public TowerType _id;
    public int _lv;
    public int _currentExp;
    public TowerGameData(TowerType id, int lv, int currentExp)
    {
        _id = id;
        _lv = lv;
        _currentExp = currentExp;
    }
}


