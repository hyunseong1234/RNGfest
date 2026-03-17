using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PickUpManager : MonoBehaviour
{
    public static PickUpManager Instance = null;
    [Header("Data List")]
    public List<PickUpData> pickUpDatas; // 각 탭의 정보 데이터

    [Header("UI Object List")]
    public List<Image> borderImages; // 실제 화면에 배치된 테두리 Image 객체들

    [Header("Display Target")]
    public Image mainBgi1;
    public Image mainBgi2;

    public TMP_Text gacha1;
    public TMP_Text gacha10;
    public Image nogoldPanel1; // 상점 버튼에 막아둘 임시 버튼 이미지
    public Image nogoldPanel10; // 상점 버튼에 막아둘 임시 버튼 이미지
    public TMP_Text shopGoldText;

    [SerializeField] private SlotMachinePresenter _slotPresenter;

    public int currentIndex = 0;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // 데이터 개수와 UI 객체 개수가 맞는지 확인
        if (pickUpDatas.Count != borderImages.Count)
        {
            Debug.LogWarning("데이터와 테두리 UI의 개수가 일치하지 않습니다.");
        }

        // 저장된 데이터가 있다면 불러오기, 없으면 0번 선택
        int lastIndex = PlayerPrefs.GetInt("SelectedPickUp", 0);
        SelectTab(lastIndex);

        GoldCheck();
    }

    public void GoldCheck()
    {
        int currentGold = PlayFabDataManager.Instance.userData._gold;

        if (currentGold < 50)
        {
            gacha1.color = Color.red; // 부족하면 레드
            nogoldPanel1.gameObject.SetActive(true); // 필요한 경우 주석 해제
        }
        else
        {
            gacha1.color = Color.black; // 충분하면 블랙
            nogoldPanel1.gameObject.SetActive(false);
        }

        if (currentGold < 500)
        {
            gacha10.color = Color.red;
            nogoldPanel1.gameObject.SetActive(true);
        }
        else
        {
            gacha10.color = Color.black;
            nogoldPanel1.gameObject.SetActive(false);
        }
        shopGoldText.text = currentGold.ToString();
    }

    public void SelectTab(int index)
    {
        if (index < 0 || index >= pickUpDatas.Count) return;

        for (int i = 0; i < borderImages.Count; i++)
        {
            if (i == index)
            {
                borderImages[i].color = pickUpDatas[i].borderColor;

                currentIndex = i;
                UpdateMainDisplay(pickUpDatas[i]);
            }
        }

        // 상태 저장
        PlayerPrefs.SetInt("SelectedPickUp", index);
    }

    private void UpdateMainDisplay(PickUpData data)
    {
        mainBgi1.sprite = data.bgi1;
        mainBgi2.sprite = data.bgi2;
    }

    public void OnClickTenGacha()
    {
        var playfab = PlayFabDataManager.Instance;
        _slotPresenter.gameObject.SetActive(true);
        playfab.userData._gold -= 500;
        playfab.SaveData();

        _slotPresenter.StartSlotMachine(10, () => { });
        GoldCheck();
    }

    public void OnClickOneGacha()
    {
        var playfab = PlayFabDataManager.Instance;
        _slotPresenter.gameObject.SetActive(true);
        playfab.userData._gold -= 50;
        playfab.SaveData();

        // 2. 그 다음 코루틴을 호출한다
        _slotPresenter.StartSlotMachine(1, () => { });
        GoldCheck();
    }
}