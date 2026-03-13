using System.Collections.Generic;
using UnityEngine;

public class TowerSlotManager : MonoBehaviour
{
    public static TowerSlotManager Instance;
    private Dictionary<TowerType, Sprite> _towerSpriteDict = new Dictionary<TowerType, Sprite>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); LoadResources(); }
        else { Destroy(gameObject); }
    }

    private void LoadResources()
    {
        _towerSpriteDict.Clear();
        // 경로 및 파일명 매칭 (TowerType Enum 이름과 동일할 것)
        Sprite[] sprites = Resources.LoadAll<Sprite>("Texture/MainLobby/TowerIcon");
        foreach (Sprite s in sprites)
        {
            if (System.Enum.TryParse(s.name, out TowerType type)) _towerSpriteDict.Add(type, s);
        }
    }

    public Sprite GetTowerSprite(TowerType type) => _towerSpriteDict.GetValueOrDefault(type);

    // [핵심] 그냥 딱 인덱스 찍어서 데이터 넣고 저장함. 꼬임 방지용 데이터 보정 포함.
    public void ChangeTowerInSlot(int slotIndex, TowerType newTower)
    {
        var userData = PlayFabDataManager.Instance.userData;
        int currentDeck = userData._currentSlot;

        // 데이터 리스트가 부족하면 None으로 채워넣기 (공유 방지 및 인덱스 에러 방지)
        var list = userData._towerSlots[currentDeck].slotTowers;
        while (list.Count <= slotIndex) list.Add(TowerType.None);

        // 해당 칸에 데이터 할당 및 서버 저장
        list[slotIndex] = newTower;
        PlayFabDataManager.Instance.SaveData();
    }
}