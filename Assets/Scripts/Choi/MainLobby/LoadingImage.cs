using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingImage : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _loadingImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Epic Animation Settings")]
    [SerializeField] private float _rotationSpeed = 480f; // 조금 더 시원하게 속도업
    [SerializeField] private float _fadeDuration = 0.3f;

    [Header("Color Strategy")]
    [SerializeField] private bool _useGradient = true;
    // 인스펙터에서 파랑 -> 청록 -> 초록 계열로 설정하면 매우 세련되어 보입니다.
    [SerializeField] private Gradient _loadingGradient;
    [SerializeField] private Color _singleColor = new Color(0.2f, 0.6f, 1f); // 기본 파랑
    [SerializeField] private float _colorSpeed = 2f;

    private Coroutine _animationCoroutine;

    private void Awake()
    {
        if (_loadingImage == null) _loadingImage = GetComponent<Image>();
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    private void OnEnable()
    {
        StopAnimation();
        _animationCoroutine = StartCoroutine(Co_PlayEpicAnimation());
    }

    private void OnDisable() => StopAnimation();

    private void StopAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private IEnumerator Co_PlayEpicAnimation()
    {
        // 1. Fade In
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1;

        // 2. Main Loop
        float timer = 0f;

        while (true)
        {
            float deltaTime = Time.unscaledDeltaTime;
            timer += deltaTime;

            // [회전] 리듬감 있는 가속 (고급스러운 느낌)
            float speedMultiplier = 1.0f + Mathf.Sin(timer * 3f) * 0.6f;
            transform.Rotate(0, 0, -_rotationSpeed * speedMultiplier * deltaTime);

            // [컬러] AI 느낌 없는 세련된 컬러링
            if (_useGradient && _loadingGradient != null)
            {
                // 그라데이션 안에서 부드럽게 왔다갔다 (0 -> 1 -> 0)
                float pingPong = Mathf.PingPong(timer * _colorSpeed, 1f);
                _loadingImage.color = _loadingGradient.Evaluate(pingPong);
            }
            else
            {
                // 단색일 경우 미세하게 밝기만 조절 (숨쉬는 느낌)
                float brightness = 0.8f + Mathf.Sin(timer * 4f) * 0.2f;
                _loadingImage.color = _singleColor * brightness;
            }

            // [스케일] 아주 미세한 호흡 (과하면 촌스러우니 0.05 정도로 축소)
            float scale = 1.0f + Mathf.Sin(timer * 4f) * 0.05f;
            transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }
    }

    public void HideLoading(System.Action onComplete = null) => StartCoroutine(Co_FadeOut(onComplete));

    private IEnumerator Co_FadeOut(System.Action onComplete)
    {
        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsed / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0;
        onComplete?.Invoke();
        gameObject.SetActive(false);
    }
}