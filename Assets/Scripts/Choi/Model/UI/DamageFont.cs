using Dev.cheol.Model;
using UnityEngine;

public class DamageFont : BaseScreenUI
{
    private float _elapsedTime = 0f;
    private float _sideForce;

    public override void OnSpawn()
    {
        base.OnSpawn();
        _elapsedTime = 0f;
        _sideForce = Random.Range(-100f, 100f);
    }

    protected override void ApplyScreenPosition(Vector3 screenPos, float distance)
    {
        // 부모의 기본 위치 세팅(머리 위 추적)을 먼저 실행해서 기준을 잡습니다.
        base.ApplyScreenPosition(screenPos, distance);

        _elapsedTime += Time.deltaTime;

        // 부모가 잡아준 기본 위치에 '통통 튀는 오프셋'만 상대적으로 더합니다.
        Vector3 bounceOffset = GetBounceOffset(_elapsedTime, 2.0f, 1.5f, _sideForce);

        // base 호출로 이미 세팅된 position에 더하기 연산!
        rectTransform.position += bounceOffset;

        // 스케일 애니메이션 (부모가 계산한 기본 baseScale 기반으로 펌핑 연출)
        float baseScale = Mathf.Clamp(15f / distance, 0.2f, 0.5f);
        float popCurve = Mathf.Sin(Mathf.Clamp01(_elapsedTime * 5f) * Mathf.PI);

        // 기본 크기에 튀는 맛(popCurve)만 살짝 얹어줌
        rectTransform.localScale = Vector3.one * (baseScale + popCurve * 0.2f);
    }
}