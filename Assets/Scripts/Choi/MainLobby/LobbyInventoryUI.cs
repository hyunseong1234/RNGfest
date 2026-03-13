using System.Collections.Generic;
using UnityEngine;

public class LobbyInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _contentParent;
    private List<SelectFramImage> _spawnedItems = new List<SelectFramImage>();

    public void RefreshInventory()
    {
        var userData = PlayFabDataManager.Instance.userData;
        if (userData == null) return;

        var towers = userData._towers;
        var currentDeck = userData._towerSlots[userData._currentSlot].slotTowers;

        for (int i = 0; i < towers.Count; i++)
        {
            // 프레임 없으면 생성
            if (i >= _spawnedItems.Count)
            {
                var go = Instantiate(_slotPrefab, _contentParent);
                _spawnedItems.Add(go.GetComponent<SelectFramImage>());
            }

            var item = _spawnedItems[i];
            item.gameObject.SetActive(true);
            item.InitSlot(towers[i]);

            // 현재 덱에 있으면 체크 표시
            item.SetEquipState(currentDeck.Contains(towers[i]._id));
        }

        for (int i = towers.Count; i < _spawnedItems.Count; i++) _spawnedItems[i].gameObject.SetActive(false);
    }
}