using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseBullet : BaseObject
    {
        public abstract void Init(Transform target, int damage, float speed = 20f);
        [SerializeField] protected FontColor _fontColor = FontColor.White;


    }


}
