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

        // 1. TowerSlotManager에서 연출용 스프라이트들 긁어오기
        PrepareSprites();

        // 2. 기존 슬롯 청소
        foreach (var slot in instantiatedSlots) Destroy(slot.gameObject);
        instantiatedSlots.Clear();

        // 3. 레이아웃 설정 (1뽑: 중앙, 10뽑: 2x5)
        SetupLayout(count);

        // 4. 슬롯 생성
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
            // 중앙 정렬을 위해 Padding이나 Spacing 조절이 필요할 수 있습니다.
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

        // 실제로 이미지가 존재하는 타워들만 모은 리스트 생성 (필터링)
        List<TowerType> validTowers = new List<TowerType>();
        foreach (TowerType type in System.Enum.GetValues(typeof(TowerType)))
        {
            if (type == TowerType.None) continue;
            if (TowerSlotManager.Instance.GetTowerSprite(type) != null) // 이미지가 있는 것만!
            {
                validTowers.Add(type);
            }
        }

        for (int i = 0; i < instantiatedSlots.Count; i++)
        {
            //랜덤 범위를 validTowers 리스트의 개수로 변경
            if (validTowers.Count == 0)
            {
                Debug.LogError("유효한 타워 이미지가 하나도 없습니다!");
                break;
            }

            TowerType randomResult = validTowers[Random.Range(0, validTowers.Count)];
            Sprite resultSprite = TowerSlotManager.Instance.GetTowerSprite(randomResult);
            string resultName = randomResult.ToString();

            Color gradeColor = Color.white;
            // (등급 로직은 나중에 validTowers 내의 타워 정보에 따라 분기 처리 추천)
            if (i == 0) gradeColor = Color.cyan;

            float spinDuration = 1.5f + (i * 0.2f);
            runningCors.Add(StartCoroutine(instantiatedSlots[i].SpinCo(resultSprite, resultName, gradeColor, spinDuration)));
        }

        foreach (var cor in runningCors) yield return cor;

        yield return new WaitForSeconds(0.5f);
        closeButton.gameObject.SetActive(true);
    }

    public void CloseProduction()
    {
        productionCanvas.SetActive(false);
        onCompleteCallback?.Invoke();
    }
}