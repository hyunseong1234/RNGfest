using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Dev.cheol.Manager
{
    /// <summary>
    /// 오브젝트 풀링용
    /// </summary>
    public class ObjectPoolingManger : BaseManager
    {
        #region 관리 할 객체 리스트 
        /// <summary>
        /// 몬스터 생성을 위한 객체 담는 오브젝트
        /// </summary>
        private Dictionary<string, Queue<BaseObject>> pushs = new Dictionary<string, Queue<BaseObject>>();

        //[SerializeField] private SerializedDictionary<string, GameObject> prefabReferences = new SerializedDictionary<string, GameObject>(); //패치경로에 담은 리스트
        #endregion



        /// <summary>
        /// 풀이된 객체들을 관리하는 부모객체 
        /// </summary>
        [SerializeField] private Transform poolTransform;

        /// <summary>
        /// 기본적으로 활성화 안된 객체들이 있는 부모객체
        /// </summary>
        [SerializeField] private Transform pushlTransform;

        public Dictionary<string, Queue<BaseObject>> Pushs { get => pushs; set => pushs = value; }
        public Transform PushlTransform { get => pushlTransform; set => pushlTransform = value; }


        #region Public Funtion

        /// <summary>
        /// 클래스용
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="path"></param>


        /// <summary>
        /// 풀링해오는 함수 (포지션은 팩토리쪽에 받기를 권장!!)
        /// </summary>
        /// <param name="pooltag">풀링할 객체 카테고리</param>
        /// <returns></returns>
        public T GetFromPool<T>(string tag = null) where T : BaseObject
        {
            string key = string.IsNullOrEmpty(tag) ? typeof(T).FullName : tag;

            if (!pushs.ContainsKey(key))
            {
                pushs.Add(key, new Queue<BaseObject>());
            }

            Queue<BaseObject> poolQueue = pushs[key];

            // 풀이 비었으면? 팩토리한테 이름만 던져서 일 시키기
            if (poolQueue.Count == 0)
            {
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                factory.LoadDataCreatedObj(key); // 팩토리가 지 리스트 뒤져서 하나 만들어 넣음
            }

            // 이제 무조건 하나는 들어있으니 꺼내기
            BaseObject objToUse = poolQueue.Count > 0 ? poolQueue.Dequeue() : null;

            if (objToUse == null) return null;

            objToUse.gameObject.SetActive(true);
            objToUse.transform.parent = poolTransform;
            objToUse.PoolTag = key;

            return objToUse as T;
        }
        /// <summary>
        /// 프리팹 전용
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public T GetFromPool<T>(BaseObject prefab) where T : BaseObject
        {
            if (prefab == null)
            {
                Debug.LogError("전달된 프리팹이 null입니다!");
                return null;
            }

            // 프리팹의 이름을 키로 사용 (보통 프리팹 이름이 고유 태그 역할을 함)
            string key = prefab.gameObject.name;

            // 1. 해당 키의 풀이 아예 없다면 새로 생성해줌 (딕셔너리 초기화)
            if (!pushs.ContainsKey(key))
            {
                pushs.Add(key, new Queue<BaseObject>());
            }

            Queue<BaseObject> poolQueue = pushs[key];
            BaseObject objToUse = null;

            if (poolQueue.Count > 0)
            {
                objToUse = poolQueue.Dequeue();
            }
            else
            {
                ServiceLocator.Instance.GetService<FactoryManager>().LoadDataCreatedObj(prefab.PoolTag, prefab);
                objToUse = poolQueue.Dequeue();
            }

            objToUse.gameObject.SetActive(true);
            objToUse.PoolTag = key; // 나중에 반납할 때를 위해 키 저장

            return objToUse as T;
        }
        /// <summary>
        /// 오브젝트 풀링용 디스트로이 대용 함수 (삭제역활)
        /// </summary>
        /// <param name="poolname"></param>
        /// <returns></returns>
        public void ReturnPool(BaseObject mObject)
        {
            string key = mObject.PoolTag;

            if (string.IsNullOrEmpty(key) || !pushs.ContainsKey(key))
            {
                Debug.LogError($"객체 '{mObject.name}'의 풀 태그('{key}')가 유효하지 않아 풀로 반환할 수 없습니다. 객체를 즉시 파괴합니다.");
                Destroy(mObject.gameObject);
                return;
            }
            mObject.gameObject.SetActive(false);
            mObject.gameObject.transform.parent = pushlTransform;
            mObject.transform.position = new Vector3(1000, 1000, 1000); //구석에 짱박아둠

            pushs[key].Enqueue(mObject);

        }

        #endregion


        #region Private Funtion


        /// <summary>
        /// 클론을 복사해와서 새로 생성
        /// </summary>
        /// <param name="pool"></param>
        //private TrashObject GetOrCreateNewObject(string key)
        //{
        //    //if (!prefabReferences.ContainsKey(key))
        //    //{
        //    //    Debug.LogError("복제할 원본 객체가 없습니다.");
        //    //    return null;
        //    //}
        //    //GameObject prefab = prefabReferences[key];

        //    //TrashObject clone = 
        //    //Instantiate(prefab, new Vector3(0, 0, 0),
        //    //Quaternion.identity, this.transform).
        //    //TryGetComponent<TrashObject>(out var @object) ? @object : null;

        //    //clone.gameObject.SetActive(false);
        //    //clone.transform.SetParent(pushlTransform);
        //    //clone.PoolTag = key;
        //    //pushs[key].Enqueue(clone);

        //    //return clone;
        //}

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }


}
