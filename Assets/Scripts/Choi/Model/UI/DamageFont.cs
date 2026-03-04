using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageFont : BaseScreenUI
{
    [SerializeField] private TextMeshProUGUI _damageText;

    [Space(10)]
    [SerializeField] private float _lifeTime = 0.7f;

    [Header("폰트 크기")]
    [SerializeField] private float _minScale = 0.01f;
    [SerializeField] private float _maxScale = 0.02f;
    [SerializeField] private float _popStrength = 0.005f;
    [SerializeField] private float _referenceDistance = 12f;

    [Header("점프 크기")]
    [SerializeField] private float _jumpHeight = 1.0f;
    [SerializeField] private float _bounceSpeed = 3.0f;
    [SerializeField] private float _sideForceRange = 1.2f;

    private Coroutine _animCoroutine;

    public override void OnSpawn()
    {
        base.OnSpawn();
        // 풀링 매니저가 혹시 부를 수도 있으니 기본 세팅만 유지
    }

    public void SetDamage(int amount, Transform targetUnit)
    {
        // 1. 데이터 세팅
        if (_damageText != null) _damageText.text = amount.ToString();
        _target = targetUnit;

        // 2. 청소 후 연출 시작 (이 코루틴이 생명주기를 관리함)
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);

        RefreshCamera();
        _animCoroutine = StartCoroutine(Co_PlayAnimation());
    }

    private IEnumerator Co_PlayAnimation()
    {
        // 카메라가 잡힐 때까지 안전하게 대기 (NPE 방어)
        while (mainCamera == null)
        {
            RefreshCamera();
            if (mainCamera == null) yield return null;
        }

        float elapsed = 0f;
        float sideForce = Random.Range(-_sideForceRange, _sideForceRange);
        transform.localScale = Vector3.zero;

        while (elapsed < _lifeTime)
        {
            // 타겟이 사라지거나 객체가 꺼지면 즉시 탈출
            if (_target == null || !gameObject.activeInHierarchy) break;

            elapsed += Time.deltaTime;

            // 1. 위치 추적 및 빌보드 (World Space)
            transform.position = _target.position + offset;
            transform.rotation = mainCamera.transform.rotation;

            // 2. 통통 튀는 연출 (인스펙터 변수 적용)
            transform.position += GetBounceOffset(elapsed, _jumpHeight, _bounceSpeed, sideForce);

            // 3. 거리 기반 스케일링 (sqrMagnitude 최적화 적용)
            Vector3 diff = mainCamera.transform.position - transform.position;
            float sqrDist = diff.sqrMagnitude;
            float dist = Mathf.Sqrt(sqrDist);

            // 인스펙터 수치 기반 계산
            float baseScale = Mathf.Clamp(_referenceDistance / (dist + 0.01f), _minScale, _maxScale);
            float pop = Mathf.Sin(Mathf.Clamp01(elapsed * 8f) * Mathf.PI);

            transform.localScale = Vector3.one * (baseScale + (pop * _popStrength));

            yield return null;
        }

        // 루프 끝나면 스스로 반납
        FinalizeAndReturn();
    }

    private void FinalizeAndReturn()
    {
        // 좀비 로직 방지
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = null;
        _target = null;

        // 매니저 업데이트 리스트에 수동 등록된 경우 제거
        var main = ServiceLocator.Instance.GetService<MainManager>();
        if (main != null && main.SpawnUI.Contains(this))
        {
            main.SpawnUI.Remove(this);
        }

        // 풀링 매니저로 반납
        var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        if (poolManager != null)
        {
            poolManager.ReturnPool(this);
        }
    }
}