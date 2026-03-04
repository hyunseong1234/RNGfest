using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

namespace Dev.cheol.Model
{
    public class BaseScreenUI : BaseObject
    {
        protected override void Awake()
        {
            base.Awake();
            IsUI = true;
        }
        public override void ObjectUpdate()
        {
            throw new System.NotImplementedException();
        }

    }
}
