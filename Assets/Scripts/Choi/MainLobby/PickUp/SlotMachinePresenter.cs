using System.Collections;
using System.Collections.Generic;
using System.Linq; // 추가
using UnityEngine;
using UnityEngine.UI;

public class SlotMachinePresenter : MonoBehaviour
{
    [Header("Hierarchy")]
    public GameObject productionCanvas;
    public Transform slotContainer;
    public GameObject slotPrefab;
    public Button closeButton;

    [Header("Layout")]
    public GridLayoutGroup gridLayout;

    private List<GachaSlot> instantiatedSlots = new List<GachaSlot>();
    private System.Action onCompleteCallback;

    // TowerSlotManager에서 쓸 수 있는 모든 스프라이트 리스트 (연출용)
    private List<Sprite> _allTowerSprites = new List<Sprite>();

    private void Awake()
    {
        productionCanvas.SetActive(false);
        closeButton.onClick.AddListener(CloseProduction);
    }

    public void StartSlotMachine(int count, System.Action onComplete)
    {
        onCompleteCallback = onComplete;
        productionCanvas.SetActive(true);
        closeButton.gameObject.SetActive(false);

        // TowerSlotManager에서 연출용 스프라이트들 긁어오기
        PrepareSprites();

        // 기존 슬롯 청소
        foreach (var slot in instantiatedSlots) Destroy(slot.gameObject);
        instantiatedSlots.Clear();

        // 레이아웃 설정 (1뽑: 중앙, 10뽑: 2x5)
        SetupLayout(count);

        // 슬롯 생성
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            GachaSlot slot = go.GetComponent<GachaSlot>();

            // TowerSlotManager에서 가져온 스프라이트들을 더미로 넣어줌
            slot.SetDummyResources(_allTowerSprites);
            instantiatedSlots.Add(slot);
        }

        StartCoroutine(SlotMachineSequence());
    }

    private void PrepareSprites()
    {
        _allTowerSprites.Clear();
        // Enum 값을 순회하며 로드된 스프라이트들을 리스트업
        foreach (TowerType type in System.Enum.GetValues(typeof(TowerType)))
        {
            if (type == TowerType.None) continue;
            Sprite s = TowerSlotManager.Instance.GetTowerSprite(type);
            if (s != null) _allTowerSprites.Add(s);
        }
    }

    private void SetupLayout(int count)
    {
        if (count == 1)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 1;
        }
        else
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5; // 2x5 형태
        }
    }

    private IEnumerator SlotMachineSequence()
    {
        List<Coroutine> runningCors = new List<Coroutine>();
        List<TowerType> validTowers = new List<TowerType>();
        List<TowerType> rewardTowers = new List<TowerType>();

        // 1. 유효 타워 필터링 (기존 동일)
        foreach (TowerType type in System.Enum.GetValues(typeof(TowerType)))
        {
            if (type == TowerType.None || type == TowerType.Max) continue;
            if (TowerSlotManager.Instance.GetTowerSprite(type) != null) validTowers.Add(type);
        }

        if (validTowers.Count == 0) { /* 에러 처리 */ yield break; }

        // 2. 가챠 결과 미리 계산 (중요: 실시간 텍스트 반영을 위해 미리 계산함)
        var playfab = PlayFabDataManager.Instance;
        // 현재 타워들의 경험치 상태를 복사본으로 들고 있음 (실시간 UI 표기용)
        Dictionary<TowerType, int> tempExpTracker = new Dictionary<TowerType, int>();
        foreach (var t in playfab.userData._towers)
        {
            tempExpTracker[t._id] = t._currentExp;
        }

        // 3. 슬롯 연출 시작
        for (int i = 0; i < instantiatedSlots.Count; i++)
        {
            TowerType randomResult = validTowers[UnityEngine.Random.Range(0, validTowers.Count)];
            rewardTowers.Add(randomResult);

            // 해당 타워의 "이전" 경험치 가져오기
            int prevExp = tempExpTracker.ContainsKey(randomResult) ? tempExpTracker[randomResult] : 0;
            // 획득 후 경험치 계산 (+5)
            int nextExp = prevExp + 5;
            tempExpTracker[randomResult] = nextExp; // 다음 슬롯에서 또 뽑힐 경우를 대비해 업데이트

            Sprite resultSprite = TowerSlotManager.Instance.GetTowerSprite(randomResult);
            string resultName = randomResult.ToString();

            // 양식: "타워이름 prev/10 -> next/10" 형태나 "next/10" 형태로 구성
            // 10은 최대치라고 가정(필요시 변수화)
            string expText = $"{nextExp} / 10";

            float spinDuration = 1.5f + (i * 0.2f);

            // ★ SpinCo가 끝난 후 텍스트를 띄우기 위해 개별 코루틴 추적
            runningCors.Add(StartCoroutine(ProcessSingleSlot(i, resultSprite, expText, resultName, spinDuration)));
        }

        // 4. 모든 슬롯 연출 대기
        foreach (var cor in runningCors) yield return cor;

        // 5. 데이터 최종 반영 및 서버 저장
        playfab.userData.AddGachaResults(rewardTowers);
        playfab.SaveData();

        yield return new WaitForSeconds(0.5f);
        closeButton.gameObject.SetActive(true);
    }

    // 슬롯 하나가 멈추고 나서 텍스트를 갱신해주는 중간 단계 코루틴
    private IEnumerator ProcessSingleSlot(int index, Sprite sprite, string expText, string name, float duration)
    {
        // 1. 슬롯 회전 연출 끝날 때까지 대기
        yield return StartCoroutine(instantiatedSlots[index].SpinCo(sprite, name, Color.white, duration));

        // 2. 회전이 멈춘 직후 해당 슬롯의 텍스트 갱신
        // GachaSlot 스크립트에 텍스트 갱신 함수가 있다고 가정 (예: SetExpText)
        instantiatedSlots[index].SetExpText(expText);
    }

    public void CloseProduction()
    {
        productionCanvas.SetActive(false);
        onCompleteCallback?.Invoke();
    }
}