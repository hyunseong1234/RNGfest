using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Outline gradeOutline; // 등급별 색상 표시용

    private List<Sprite> allDummySprites; // 회전 연출용 더미 이미지 리스트

    public void Awake()
    {
        // 리소스가 없으므로 초기엔 가려둠
        itemIcon.gameObject.SetActive(false);
        if (itemNameText) itemNameText.text = "";
    }

    // 연출용 더미 데이터 설정
    public void SetDummyResources(List<Sprite> sprites)
    {
        allDummySprites = sprites;
    }

    // 슬롯 머신 회전 코루틴
    public IEnumerator SpinCo(Sprite finalSprite, string finalName, Color gradeColor, float duration)
    {
        itemIcon.gameObject.SetActive(true);
        if (itemNameText) itemNameText.text = "???";

        // 데이터 검증 로그 추가
        if (finalSprite == null) Debug.LogError($"{finalName}의 스프라이트가 NULL입니다!");

        float elapsed = 0f;
        float lastSwapTime = 0f;
        int dummyIndex = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentSpeed = Mathf.Lerp(0.05f, 0.2f, elapsed / duration);

            if (elapsed - lastSwapTime > currentSpeed && allDummySprites.Count > 0)
            {
                itemIcon.sprite = allDummySprites[dummyIndex % allDummySprites.Count];
                dummyIndex++;
                lastSwapTime = elapsed;

                // Invoke 대신 직접 처리
                transform.localScale = Vector3.one * 1.05f;
            }

            // 부드럽게 스케일 복구
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 10f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        itemIcon.sprite = finalSprite;
        if (itemNameText) itemNameText.text = finalName;
        if (gradeOutline) gradeOutline.effectColor = gradeColor;

        yield return StartCoroutine(PunchScale(1.3f, 0.2f));
    }
    private void ResetScale() { transform.localScale = Vector3.one; }

    IEnumerator PunchScale(float punch, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(punch, 1f, elapsed / time);
            transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}