using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DraggableIcon))]
[RequireComponent(typeof(CanvasGroup))]
public class SelectFramImage : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text _lvText;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _towerImage;
    [SerializeField] private Image _filledImage;
    [SerializeField] private Image _arrowIcon;

    [Header("State Elements")]
    [SerializeField] private CanvasGroup _canvasGroup;


    TowerGameData _towerGameData;
    private TowerType _towerType = TowerType.None;

    private void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void InitSlot(TowerGameData data)
    {
        _towerType = data._id;
        int lv = data._lv;
        int exp = data._currentExp;
        int maxExp = 10 + (lv * 5);
        _towerGameData = data; //정보 일단 담고있기

        if (_lvText) _lvText.text = $"Lv.{lv}";
        if (_countText) _countText.text = $"{exp}/{maxExp}";
        if (_filledImage) _filledImage.fillAmount = (float)exp / maxExp;
        if (_arrowIcon) _arrowIcon.gameObject.SetActive(exp >= maxExp);

        if (_towerImage)
            _towerImage.sprite = TowerSlotManager.Instance.GetTowerSprite(_towerType);
    }

    /// <summary>
    /// 현재 덱(프리셋)에 포함 여부에 따라 슬롯 자체를 끄거나 켬
    /// </summary>
    public void SetEquipState(bool isEquipped)
    {
        // 장착 중(isEquipped == true)이면 오브젝트를 끄고, 아니면 켭니다.
        this.gameObject.SetActive(!isEquipped);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그 중이 아닐 때만 정보창 오픈
        if (!eventData.dragging)
        {
            if (TowerSlotManager.Instance._towerPanel == null)
            {
                TowerSlotManager.Instance._towerPanel = Object.FindAnyObjectByType<TowerPanel>();

            }
            TowerSlotManager.Instance._towerPanel.gameObject.SetActive(true);
        }
    }


    public void OpenInfo()
    {
        if (TowerSlotManager.Instance._towerPanel == null)
        {
            TowerSlotManager.Instance._towerPanel = Object.FindAnyObjectByType<TowerPanel>();
        }
        var towerPanel = TowerSlotManager.Instance._towerPanel;
        towerPanel.SetInfo(_towerGameData);
        TowerSlotManager.Instance._towerPanel.gameObject.SetActive(true);
    }

    public TowerType GetTowerType() => _towerType;
}