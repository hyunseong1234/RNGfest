using System.Collections.Generic;
using UnityEngine;

// 타워 개별 등급 데이터
[System.Serializable]
public class TowerStat
{
    public int rank;
    public float attack;
    public float speed;
    public float range;
    public List<float> specialValues = new List<float>();
}

// 타워 데이터 SO
[CreateAssetMenu(fileName = "NewTowerData", menuName = "Data/TowerData")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public List<TowerStat> stats = new List<TowerStat>();
}