using UnityEngine;

namespace Dev.cheol.Model
{
    public class BaseObject : MonoBehaviour
    {
        [SerializeField] private string _poolTag;
        public string PoolTag { get => _poolTag; set => _poolTag = value; }

    }
}
