using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PickUpManager : MonoBehaviour
{

    [Header("Data List")]
    public List<PickUpData> pickUpDatas; // 각 탭의 정보 데이터

    [Header("UI Object List")]
    public List<Image> borderImages; // 실제 화면에 배치된 테두리 Image 객체들

    [Header("Display Target")]
    public Image mainBgi1;
    public Image mainBgi2;


    [SerializeField] private SlotMachinePresenter _slotPresenter;

    public int currentIndex = 0;

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

        // 4. 상태 저장
        PlayerPrefs.SetInt("SelectedPickUp", index);
    }

    private void UpdateMainDisplay(PickUpData data)
    {
        mainBgi1.sprite = data.bgi1;
        mainBgi2.sprite = data.bgi2;
    }

    public void OnClickTenGacha()
    {

        // 1. 부모인 Shop Panel(또는 SlotMachinePresenter 자체)을 먼저 켠다
        // 만약 Shop Panel을 참조하고 있다면 그것을 SetActive(true) 하세요.
        _slotPresenter.gameObject.SetActive(true);

        // 2. 그 다음 코루틴을 호출한다
        _slotPresenter.StartSlotMachine(10, () => { });

    }
}