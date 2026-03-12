using Dev.cheol.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseBullet : BaseObject
    {
        [Header("Common Hit Effect")]
        [SerializeField] protected BaseObject _hitEffectPrefab; // ¸ðµç ÃÑ¾ËÀÌ °øÀ¯ÇÏ´Â ÀÌÆåÆ® ½½·Ô

        protected float _speed = 20f;
        [SerializeField] protected float _damage = 10;
        public abstract void Init(Transform target, float damage, float speed = 20f);
        [SerializeField] protected FontColor _fontColor = FontColor.White;

        protected void SpawnHitEffect(Vector3 position)
        {
            if (_hitEffectPrefab == null) return;

        }
        protected virtual void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }
    }


}
