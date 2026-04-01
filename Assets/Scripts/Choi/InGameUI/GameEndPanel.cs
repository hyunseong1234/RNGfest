using Dev.cheol.Manager;
using Dev.jeon.Manager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndPanel : MonoBehaviour
{
    #region Inspector Fields
    [Header("UI References")]
    [SerializeField] private Image goldImage;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private Button lobbyGoButton;

    [Header("Reward Settings")]
    [SerializeField] private int baseReward = 100;
    [SerializeField] private int rewardStep = 50;
    [SerializeField] private int maxWave = 50;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float punchDuration = 0.1f;
    #endregion

    #region Private Variables
    private Coroutine rewardAnimationCoroutine;
    private Vector3 originalGoldImageScale;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (goldImage != null) originalGoldImageScale = goldImage.transform.localScale;
    }

    private void OnEnable()
    {
        ResetUI();
        CalculateAndDisplayReward();
    }

    private void OnDisable()
    {
        if (rewardAnimationCoroutine != null)
            StopCoroutine(rewardAnimationCoroutine);

        if (goldImage != null) goldImage.transform.localScale = originalGoldImageScale;
    }
    #endregion

    #region Reward Logic
    void ResetUI()
    {
        goldText.text = "0";
        if (goldImage != null) goldImage.transform.localScale = originalGoldImageScale;
        if (lobbyGoButton != null) lobbyGoButton.gameObject.SetActive(false);
    }

    void CalculateAndDisplayReward()
    {
        int waveIndex = ServiceLocator.Instance.GetService<WaveManager>().CurrentWaveIndex;
        if (waveText != null) waveText.text = $"WAVE {waveIndex}";

        int rewardTier = waveIndex / 5;
        int finalReward = (waveIndex < 5) ? 0 : baseReward + (rewardTier * rewardStep);
        if (waveIndex >= maxWave)
        {
            finalReward = (int)(finalReward * 1.5f);
        }

        var playfab = PlayFabDataManager.Instance;
        playfab.userData._gold += finalReward;
        playfab.SaveData();

        if (finalReward > 0)
        {
            if (rewardAnimationCoroutine != null) StopCoroutine(rewardAnimationCoroutine);
            rewardAnimationCoroutine = StartCoroutine(Co_PlayRewardAnimation(finalReward));
        }
        else
        {
            if (lobbyGoButton != null) lobbyGoButton.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Animation Coroutines
    IEnumerator Co_PlayRewardAnimation(int targetGold)
    {
        float elapsedTime = 0f;
        int currentDisplayedGold = 0;
        Coroutine punchCoroutine = StartCoroutine(Co_RepeatPunchImage());

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // TimeScale 0 대응
            float progress = Mathf.Clamp01(elapsedTime / duration);
            progress = 1f - Mathf.Pow(1f - progress, 3f);

            currentDisplayedGold = (int)Mathf.Lerp(0, targetGold, progress);
            goldText.text = currentDisplayedGold.ToString("N0");

            yield return null;
        }

        goldText.text = targetGold.ToString("N0");
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        if (goldImage != null) goldImage.transform.localScale = originalGoldImageScale;
        if (lobbyGoButton != null) lobbyGoButton.gameObject.SetActive(true);

        rewardAnimationCoroutine = null;
    }

    IEnumerator Co_RepeatPunchImage()
    {
        if (goldImage == null) yield break;
        Vector3 targetScale = originalGoldImageScale * punchScale;

        while (true)
        {
            float time = 0f;
            while (time < punchDuration)
            {
                time += Time.unscaledDeltaTime;
                goldImage.transform.localScale = Vector3.Lerp(originalGoldImageScale, targetScale, time / punchDuration);
                yield return null;
            }

            time = 0f;
            while (time < punchDuration)
            {
                time += Time.unscaledDeltaTime;
                goldImage.transform.localScale = Vector3.Lerp(targetScale, originalGoldImageScale, time / punchDuration);
                yield return null;
            }
        }
    }
    #endregion

    #region UI Events
    public void _OnClickGameEnd()
    {
        Time.timeScale = 1f; // 다음 씬을 위해 시간 복구
        SceneManager.LoadScene(1);
    }
    #endregion
}