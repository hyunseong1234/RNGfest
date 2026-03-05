using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseBullet : BaseObject
    {
        protected float _speed = 20f;
        [SerializeField] protected float _damage = 10;
        public abstract void Init(Transform target, float damage, float speed = 20f);
        [SerializeField] protected FontColor _fontColor = FontColor.White;


    }


}
