using Dev.cheol.Model;
using Dev.jeon.Bullet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class ObjectPoolingManger : BaseManager
    {
        private Dictionary<string, Queue<BaseObject>> pushs = new Dictionary<string, Queue<BaseObject>>();

        [SerializeField] private Transform poolTransform;
        [SerializeField] private Transform pushlTransform;

        // [추가] UI 전용 부모 객체 필드
        [SerializeField] private Transform poolTransformUI;
        [SerializeField] private Transform pushlTransformUI;

        public Dictionary<string, Queue<BaseObject>> Pushs { get => pushs; set => pushs = value; }
        public Transform PushlTransform { get => pushlTransform; set => pushlTransform = value; }
        public Transform PoolTransformUI { get => poolTransformUI; set => poolTransformUI = value; }
        public Transform PushlTransformUI { get => pushlTransformUI; set => pushlTransformUI = value; }

        public T GetFromPool<T>(string tag = null) where T : BaseObject
        {
            string key = string.IsNullOrEmpty(tag) ? typeof(T).FullName : tag;

            if (!pushs.ContainsKey(key)) pushs.Add(key, new Queue<BaseObject>());
            Queue<BaseObject> poolQueue = pushs[key];

            if (poolQueue.Count == 0)
            {
                var factory = ServiceLocator.Instance.GetService<FactoryManager>();
                factory.LoadDataCreatedObj(key);
            }

            BaseObject objToUse = poolQueue.Count > 0 ? poolQueue.Dequeue() : null;
            if (objToUse == null) return null;

            objToUse.gameObject.SetActive(true);
            objToUse.PoolTag = key;

            //UI 여부에 따른 부모 설정 분기
            if (objToUse.transform is RectTransform)
            {
                objToUse.transform.SetParent(poolTransformUI, false);
                ((RectTransform)objToUse.transform).SetAsLastSibling();
            }
            else
            {
                objToUse.transform.SetParent(poolTransform);
            }

            return objToUse as T;
        }

        public T GetFromPool<T>(BaseObject prefab) where T : BaseObject
        {
            if (prefab == null) return null;
            string key = prefab.gameObject.name;

            if (!pushs.ContainsKey(key)) pushs.Add(key, new Queue<BaseObject>());
            Queue<BaseObject> poolQueue = pushs[key];
            BaseObject objToUse = null;


            if (poolQueue.Count > 0) objToUse = poolQueue.Dequeue();
            else
            {
                ServiceLocator.Instance.GetService<FactoryManager>().LoadDataCreatedObj(prefab.PoolTag, prefab);
                if (poolQueue.Count > 0) objToUse = poolQueue.Dequeue();
                else
                {
                    objToUse = Instantiate(prefab);
                    objToUse.name = key;
                }
            }

            objToUse.gameObject.SetActive(true);
            objToUse.PoolTag = key;

            // UI 여부에 따른 부모 설정 분기
            if (objToUse.transform is RectTransform)
            {
                objToUse.transform.SetParent(poolTransformUI, false);
                ((RectTransform)objToUse.transform).SetAsLastSibling();
            }
            else
            {
                objToUse.transform.SetParent(poolTransform);
            }

            return objToUse as T;
        }

        public void ReturnPool(BaseObject mObject)
        {
            string key = mObject.PoolTag;

            if (string.IsNullOrEmpty(key) || !pushs.ContainsKey(key))
            {
                Debug.LogError($"객체 '{mObject.name}'의 풀 태그 유효하지 않음. 파괴합니다.");
                Destroy(mObject.gameObject);
                return;
            }

            mObject.gameObject.SetActive(false);

            // [추가] UI 여부에 따른 비활성화 부모 설정 분기
            if (mObject.transform is RectTransform)
            {
                mObject.transform.SetParent(pushlTransformUI, false);
            }
            else
            {
                mObject.transform.SetParent(pushlTransform);
                mObject.transform.position = new Vector3(1000, 1000, 1000);
            }

            pushs[key].Enqueue(mObject);
        }

        public override void HandleEvent(string data) { }

        internal void ReturnPool(BossBullet skillBullet)
        {
            throw new NotImplementedException();
        }
    }
}