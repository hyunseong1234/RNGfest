using Dev.cheol.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseBullet : BaseObject
    {
        [Header("Common Hit Effect")]
        [SerializeField] protected BaseObject _hitEffectPrefab; // 모든 총알이 공유하는 이펙트 슬롯

        protected float _speed = 20f;
        [SerializeField] protected float _damage = 10;
        public abstract void Init(Transform target, float damage, float speed = 20f);
        [SerializeField] protected FontColor _fontColor = FontColor.White;

        protected void SpawnHitEffect(Vector3 position)
        {
            if(_hitEffectPrefab == null) return;

            // 서비스 로케이터로 풀링 매니저를 가져옵니다.
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (pool == null) return;

            // 풀에서 이펙트를 꺼냅니다.
            BaseObject effect = pool.GetFromPool<BaseObject>(_hitEffectPrefab);

            if (effect != null)
            {
                effect.transform.position = position;
                effect.gameObject.SetActive(true);
                // Tip: 이펙트 프리팹 자체에 일정 시간 뒤 스스로 ReturnPool 되는 스크립트가 있어야 합니다.
            }
        }
        protected virtual void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }
    }


}
