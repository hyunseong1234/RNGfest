using UnityEngine;

namespace Dev.cheol.Model
{
    public abstract class BaseObject : MonoBehaviour
    {
        [SerializeField] private string _poolTag;
        public string PoolTag { get => _poolTag; set => _poolTag = value; }



        /// <summary>
        /// 없데이트용 
        /// </summary>
        public abstract void ObjectUpdate();
    }
}
