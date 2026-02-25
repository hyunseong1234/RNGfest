using Dev.cheol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class FactoryManager : BaseManager
    {
        string[] tagNames;
        [SerializeField] private ObjectPoolingManger poolingManger = null;
        [SerializeField] private BaseObject[] _prefabs = null;

        //[SerializeField] private GameObject _playerPrefab = null;

        private void Awake()
        {
            StartSetting();

            // 첫 번째 경로 로드
            var enemyPrefabs = Resources.LoadAll<BaseObject>("Prefabs/CYC/Enemy");
            // 두 번째 경로 로드
            var towerPrefabs = Resources.LoadAll<BaseObject>("Prefabs/CYC/Tower");
            // 두 배열을 합쳐서 _prefabs에 할당
            _prefabs = enemyPrefabs.Concat(towerPrefabs).ToArray();


        }


        private void StartSetting()
        {
            if (poolingManger != null) return;
            poolingManger = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        }


        public void LoadDataCreatedObj<T>(string tag, T path) where T : BaseObject
        {
            T obj = Instantiate(path);
            obj.gameObject.SetActive(false);
            obj.PoolTag = tag;

            if (!poolingManger.Pushs.ContainsKey(tag))
            {
                Debug.Log($"[PoolManager] 신규 태그 생성됨: {tag}");
                poolingManger.Pushs.Add(tag, new Queue<BaseObject>()); // 여기서 Queue를 생성해야 KeyNotFound가 안 뜸
            }
            obj.transform.parent = poolingManger.PushlTransform; //부모설정
            poolingManger.Pushs[tag].Enqueue(obj); // 큐 삽입
        }



        /// <summary>
        /// 미리 생성해주는 코드 프리팹 버전
        /// </summary>
        /// <param name="count"></param>
        /// <param name="tag"></param>
        /// <param name="file"></param>
        public void SettingObject(int count, string tag, BaseObject file)
        {
            for (int i = 0; i < count; i++)
            {
                LoadDataCreatedObj<BaseObject>(tag, file);
            }
        }
        public void SettingObject(int count, string tag, int fileIndex)
        {
            for (int i = 0; i < count; i++)
            {
                LoadDataCreatedObj<BaseObject>(tag, _prefabs[fileIndex]);
            }

        }
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

    }//END Class
}
