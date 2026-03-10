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
            if (_hitEffectPrefab == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var effect = pool.GetFromPool<BaseObject>(_hitEffectPrefab);

            if (effect != null)
            {
                // 약간의 높이 보정(0.5f)을 포함하여 위치 설정
                effect.transform.position = position + new Vector3(0, 0.5f, 0);
            }
        }
        protected virtual void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }
    }


}
