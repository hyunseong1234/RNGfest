using Dev.cheol.Manager;
using Dev.cheol.Model;
using UnityEngine;

public class EffectAutoReturn : BaseObject
{
    [Header("이펙트가 유지될 시간")]
    [SerializeField] private float _duration = 1.0f;

    private void OnEnable()
    {
        // 켜지자마자 _duration 초 후에 ReturnToPool 실행
        Invoke(nameof(ReturnToPool), _duration);
    }

    private void ReturnToPool()
    {
        // 서비스 로케이터를 통해 풀에 나 자신을 반납
        var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        if (pool != null)
        {
            pool.ReturnPool(this);
        }
    }

    private void OnDisable()
    {
        // 혹시 모르니 꺼질 때 예약된 Invoke 취소
        CancelInvoke();
    }

    public override void ObjectUpdate() { } // BaseObject 추상 함수 구현
}