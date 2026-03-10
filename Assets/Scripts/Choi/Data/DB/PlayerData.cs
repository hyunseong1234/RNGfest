using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int gold;
    public int highSelectionStage; // 최고 클리어 스테이지
    public List<int> towerLevels = new List<int>(); // 타워들 레벨 (예: 1번타워 5렙, 2번타워 3렙...)
}