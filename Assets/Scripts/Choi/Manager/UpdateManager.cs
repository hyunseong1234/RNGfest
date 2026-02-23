using UnityEngine;

namespace Dev.cheol.Manager
{

    public abstract class UpdateManager : BaseManager
    {
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }
        public abstract void ManagerUpdate();



    }
}
