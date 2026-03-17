using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Outline gradeOutline; // 등급별 색상 표시용

    private List<Sprite> allDummySprites; // 회전 연출용 더미 이미지 리스트

    public void Awake()
    {
        // 리소스가 없으므로 초기엔 가려둠
        itemIcon.gameObject.SetActive(false);
        if (expText) expText.text = "";
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
        if (expText) expText.text = "???";

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
        if (expText) expText.text = finalName;
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

    private Coroutine scaleCor;

    public void SetExpText(string text)
    {
        if (expText == null) return;

        expText.text = text;

        // 기존에 돌고 있던 연출이 있다면 멈추고 새로 시작
        if (scaleCor != null) StopCoroutine(scaleCor);
        scaleCor = StartCoroutine(ScaleUpEffect(1.5f, 0.2f));
    }

    private IEnumerator ScaleUpEffect(float targetScale, float duration)
    {
        Vector3 initialScale = Vector3.one;
        Vector3 peakScale = Vector3.one * targetScale;

        // 1. 커지는 단계
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            expText.transform.localScale = Vector3.Lerp(initialScale, peakScale, elapsed / duration);
            yield return null;
        }

        // 2. 다시 원래대로 돌아오는 단계 (약간 더 빠르게)
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            expText.transform.localScale = Vector3.Lerp(peakScale, initialScale, elapsed / (duration * 0.5f));
            yield return null;
        }

        expText.transform.localScale = initialScale;
    }
}