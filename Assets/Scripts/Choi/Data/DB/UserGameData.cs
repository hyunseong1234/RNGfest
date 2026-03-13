
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


