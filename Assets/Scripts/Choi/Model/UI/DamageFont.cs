using Dev.cheol.Manager;
using Dev.cheol.Model;
using TMPro;
using UnityEngine;

public class DamageFont : BaseScreenUI
{
    [Header("Text Settings")]
    [SerializeField] private TextMeshProUGUI _damageText;

    private float _elapsedTime = 0f;
    private float _sideForce;
    private float _lifeTime = 1.0f;

    // 캐싱을 통해 매 프레임 GetService 호출을 방지합니다.
    private MainManager _mainManager;
    private ObjectPoolingManger _poolManager;

    protected override void Awake()
    {
        base.Awake();
        // 자주 쓰이는 매니저는 미리 캐싱합니다.
        _mainManager = ServiceLocator.Instance.GetService<MainManager>();
        _poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        _elapsedTime = 0f;
        _sideForce = Random.Range(-80f, 80f); // 튀는 범위를 약간 조절
        rectTransform.localScale = Vector3.zero;
    }

    public void SetDamage(int amount, Transform targetUnit)
    {
        if (_damageText != null) _damageText.text = amount.ToString();
        _target = targetUnit;
    }

    protected override void ApplyScreenPosition(Vector3 screenPos, float distance)
    {
        // 1. 부모의 좌표 갱신 실행
        base.ApplyScreenPosition(screenPos, distance);

        // [최적화] Update 주기에 맞춰 DeltaTime 보정
        _elapsedTime += Time.deltaTime * 2f;

        // 2. 통통 튀는 연출
        Vector3 bounceOffset = GetBounceOffset(_elapsedTime, 2.0f, 1.5f, _sideForce);
        rectTransform.position += bounceOffset;

        // 3. 스케일 및 팝업 연출
        // 나눗셈 최적화를 위해 distance에 아주 작은 값을 더해 0나누기 방지
        float baseScale = Mathf.Clamp(15f / (distance + 0.01f), 0.2f, 0.5f);
        float popCurve = Mathf.Sin(Mathf.Clamp01(_elapsedTime * 5f) * Mathf.PI);
        rectTransform.localScale = Vector3.one * (baseScale + popCurve * 0.2f);

        // 4. 수명 관리
        if (_elapsedTime >= _lifeTime)
        {
            // 캐싱된 매니저를 사용하여 성능 향상
            if (_mainManager != null) _mainManager.SpawnUI.Remove(this);
            _target = null;
            if (_poolManager != null) _poolManager.ReturnPool(this);
        }
    }
}