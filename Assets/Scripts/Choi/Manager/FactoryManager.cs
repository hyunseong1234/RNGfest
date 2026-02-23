using System;
using System.Collections.Generic;
using UnityEngine;
using Dev.cheol.Model;

namespace Dev.cheol.Manager
{

    public class FactoryManager : BaseManager
    {
        string[] tagNames;
        [SerializeField] private ObjectPoolingManger poolingManger = null;
        [SerializeField] private BaseObject[] prefabs = null;


        //[SerializeField] private GameObject _playerPrefab = null;



        private void Awake()
        {
            prefabs = Resources.LoadAll<BaseObject>("Prefab/cyc/TrashObject");

            poolingManger = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        }


        public void StartSetting()
        {
            SettingObject(30, "쓰래기", prefabs[0]);
            SettingObject(50, "쓰래기", prefabs[1]);
            SettingObject(50, "쓰래기", prefabs[2]);

            SettingObject(5, "버블", prefabs[3]);

        }


        public void LoadDataCreatedObj<T>(string tag, T path) where T : BaseObject
        {
            T obj = Instantiate(path);
            obj.gameObject.SetActive(false);

            if (!poolingManger.Pushs.ContainsKey(tag))
            {
                Debug.Log($"[PoolManager] 신규 태그 생성됨: {tag}");
                poolingManger.Pushs.Add(tag, new Queue<BaseObject>()); // 여기서 Queue를 생성해야 KeyNotFound가 안 뜸
            }
            poolingManger.Pushs[tag].Enqueue(obj);
            obj.transform.parent = poolingManger.PushlTransform; //부모설정
        }




        private void SettingObject(int count, string tag, BaseObject file)
        {
            for (int i = 0; i < count; i++)
            {
                LoadDataCreatedObj<BaseObject>(tag, file);
            }
        }
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

    }//END Class
}
