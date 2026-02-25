
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class UIManager : BaseManager
    {
        [SerializeField] List<UIObject> _uiobjs = new List<UIObject>();

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }


    }

}
