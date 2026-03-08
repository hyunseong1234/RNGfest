using UnityEngine;
using Dev.cheol.Manager; // ServiceLocator와 Manager가 있는 곳
using Dev.cheol.Model;   // BaseObject가 있는 곳

public class EffectAutoReturn : BaseObject
{
    [SerializeField] private float _duration = 0.5f; // 이펙트가 보여질 시간

    private void OnEnable()
    {
        // 1. 이름표 정제 (매니저가 알아볼 수 있게)
        if (string.IsNullOrEmpty(PoolTag))
        {
            PoolTag = name.Replace("(Clone)", "");
        }

        // 2. _duration 초 뒤에 ReturnToPool 함수를 실행해라!
        Invoke(nameof(ReturnSelf), _duration);
    }

    private void ReturnSelf()
    {
        // 3. 나 자신을 풀로 반납 (총알과는 별개로 움직임)
        var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        if (pool != null)
        {
            pool.ReturnPool(this);
        }
    }

    private void OnDisable()
    {
        // 혹시 모르니 꺼질 때 예약된 알람을 취소합니다.
        CancelInvoke();
    }

    public override void ObjectUpdate() { }
}