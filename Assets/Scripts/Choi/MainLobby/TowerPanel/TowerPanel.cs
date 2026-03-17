using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerPanel : MonoBehaviour
{
    [SerializeField] private Image _towerIcon;
    [SerializeField] private List<ImageSlotBar> _currentLvimageBar;
    [SerializeField] private Image _notUpgradeImage;
    [SerializeField] private Image _towerExpImage;
    [SerializeField] private TMP_Text _towerExpText;
    // [추가] 현재 선택된 타워의 데이터를 기억하기 위한 변수
    private TowerGameData _currentData;

    private void OnEnable()
    {
        // 리스트가 비어있으면 자식들로부터 자동으로 채워줌
        if (_currentLvimageBar == null || _currentLvimageBar.Count == 0)
        {
            _currentLvimageBar = GetComponentsInChildren<ImageSlotBar>().ToList();
        }
    }

    /// <summary>
    /// 외부(슬롯 클릭 등)에서 타워 정보를 전달받아 창을 띄울 때 호출
    /// </summary>
    public void SetInfo(TowerGameData data)
    {
        if (data == null) return;

        _currentData = data; // 데이터 캐싱
        RefreshUI();         // UI 갱신 로직 실행
    }

    /// <summary>
    /// 강화 버튼에 연결할 함수 (버튼의 OnClick 이벤트에 등록하세요)
    /// </summary>
    public void OnClickUpgrade()
    {
        if (_currentData == null) return;

        int maxExp = 10 + (_currentData._lv * 5);

        // 경험치가 충분한지 최종 체크
        if (_currentData._currentExp >= maxExp)
        {
            _currentData._currentExp -= maxExp;
            _currentData._lv++;

            RefreshUI();
            TowerPresetManager.Instance.RefreshAll();

            PlayFabDataManager.Instance.SaveData();

            Debug.Log($"[{_currentData._id}] 강화 성공! 현재 Lv.{_currentData._lv}");
        }
    }


    /// <summary>
    /// 현재 데이터(_currentData)를 바탕으로 모든 UI 요소를 새로고침
    /// </summary>
    private void RefreshUI()
    {
        if (_currentData == null) return;

        // 타워 아이콘 교체
        if (_towerIcon != null)
            _towerIcon.sprite = TowerSlotManager.Instance.GetTowerSprite(_currentData._id);

        // 레벨 바 (슬롯) 색상 갱신
        for (int i = 0; i < _currentLvimageBar.Count; i++)
        {
            bool isReached = (i == _currentData._lv - 1);
            _currentLvimageBar[i].SetSlotColor(isReached);
        }

        // 경험치 계산 및 UI 적용
        int currentExp = _currentData._currentExp;
        int maxExp = 10 + (_currentData._lv * 5); // 레벨 비례 공식 적용

        // 경험치 텍스트 (예: 5 / 15)
        if (_towerExpText != null)
        {
            _towerExpText.text = $"{currentExp} / {maxExp}";
        }

        // 경험치 이미지 (Fill Amount 조정)
        if (_towerExpImage != null)
        {
            // 0.0 ~ 1.0 사이 값으로 변환하여 적용
            _towerExpImage.fillAmount = (float)currentExp / maxExp;
        }

        // 강화 가능 여부에 따른 막기 이미지 처리
        bool cannotUpgrade = currentExp < maxExp;
        if (_notUpgradeImage != null)
        {
            _notUpgradeImage.gameObject.SetActive(cannotUpgrade);
        }
    }
}