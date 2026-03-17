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
        //초기 설정 및 가챠 결과 저장용 리스트
        List<Coroutine> runningCors = new List<Coroutine>();
        List<TowerType> validTowers = new List<TowerType>();
        List<TowerType> rewardTowers = new List<TowerType>();

        // 2. 실제로 이미지가 존재하는 타워들만 필터링 (기존 로직)
        foreach (TowerType type in System.Enum.GetValues(typeof(TowerType)))
        {
            if (type == TowerType.None || type == TowerType.Max) continue;

            // 타워 슬롯 매니저에서 스프라이트가 있는지 확인
            if (TowerSlotManager.Instance.GetTowerSprite(type) != null)
            {
                validTowers.Add(type);
            }
        }

        // 예외 처리: 뽑을 수 있는 타워가 없는 경우
        if (validTowers.Count == 0)
        {
            UnityEngine.Debug.LogError("가챠 풀에 유효한 타워가 하나도 없습니다!");
            closeButton.gameObject.SetActive(true);
            yield break;
        }

        //  슬롯 개수만큼 랜덤 결과 생성 및 연출 시작
        for (int i = 0; i < instantiatedSlots.Count; i++)
        {
            // 랜덤 타워 결정 및 결과 리스트에 추가
            TowerType randomResult = validTowers[UnityEngine.Random.Range(0, validTowers.Count)];
            rewardTowers.Add(randomResult);

            // UI 연출용 데이터 준비
            Sprite resultSprite = TowerSlotManager.Instance.GetTowerSprite(randomResult);
            string resultName = randomResult.ToString();

            // 첫 번째 슬롯만 강조 색상 (원하시는 대로 수정 가능)
            Color gradeColor = (i == 0) ? Color.cyan : Color.white;

            // 슬롯마다 회전 시간을 다르게 하여 순차적으로 멈추는 느낌 전달
            float spinDuration = 1.5f + (i * 0.2f);

            // 슬롯 컴포넌트의 SpinCo 코루틴 실행 및 리스트에 저장
            runningCors.Add(StartCoroutine(instantiatedSlots[i].SpinCo(resultSprite, resultName, gradeColor, spinDuration)));
        }

        // 모든 슬롯의 회전 연출이 끝날 때까지 대기
        foreach (var cor in runningCors)
        {
            yield return cor;
        }

        // 데이터 반영 및 서버 저장 (필살기 콤보)
        var playfab = PlayFabDataManager.Instance;

        // 타워 데이터 추가/경험치 로직 실행
        playfab.userData.AddGachaResults(rewardTowers);

        playfab.SaveData();

        // 마무리 연출 후 닫기 버튼 활성화
        yield return new WaitForSeconds(0.5f);
        closeButton.gameObject.SetActive(true);

        UnityEngine.Debug.Log("가챠 연출 및 데이터 저장 완료!");
    }

    public void CloseProduction()
    {
        productionCanvas.SetActive(false);
        onCompleteCallback?.Invoke();
    }
}