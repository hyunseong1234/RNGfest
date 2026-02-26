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
        [SerializeField] private BaseObject[] _prefabs_Twoer = null;
        [SerializeField] private BaseObject[] _prefabs_Enmey = null;
        [SerializeField] private BaseObject[] _prefabs_Bullet = null;

        [SerializeField] private BaseObject[] _prefabs = null;

        public BaseObject[] Prefabs_Twoer { get => _prefabs_Twoer; set => _prefabs_Twoer = value; }
        public BaseObject[] Prefabs_Enmey { get => _prefabs_Enmey; set => _prefabs_Enmey = value; }
        public BaseObject[] Prefabs_Bullet { get => _prefabs_Bullet; set => _prefabs_Bullet = value; }



        //[SerializeField] private GameObject _playerPrefab = null;

        private void Awake()
        {
            StartSetting();

            _prefabs_Enmey = Resources.LoadAll<BaseObject>("Prefabs/CYC/Enemy");
            _prefabs_Twoer = Resources.LoadAll<BaseObject>("Prefabs/CYC/Tower");
            _prefabs_Bullet = Resources.LoadAll<BaseObject>("Prefabs/CYC/Bullet");
            _prefabs = _prefabs_Enmey.Concat(_prefabs_Twoer).Concat(_prefabs_Bullet).ToArray();
        }

        private void Start()
        {
            SettingObject(8, _prefabs_Enmey);
            SettingObject(20, _prefabs_Twoer);
        }

        private void StartSetting()
        {
            if (poolingManger != null) return;
            poolingManger = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        }
        public void LoadDataCreatedObj(string tag) // 프리팹 안 줘도 이름만 주면 알아서 함
        {
            // 1. 팩토리가 쥐고 있는 리스트에서 이름 일치하는 놈 탐색
            BaseObject target = _prefabs.FirstOrDefault(p => p.gameObject.name == tag);

            if (target == null)
            {
                Debug.LogError($"[Factory] '{tag}' 프리팹이 리스트에 없습니다. 리소스 경로 확인하세요.");
                return;
            }

            // 2. 찾았으면 바로 생성해서 풀에 꽂아넣기
            BaseObject obj = Instantiate(target);
            obj.gameObject.SetActive(false);
            obj.PoolTag = tag;

            if (!poolingManger.Pushs.ContainsKey(tag))
            {
                poolingManger.Pushs.Add(tag, new Queue<BaseObject>());
            }

            obj.transform.parent = poolingManger.PushlTransform;
            poolingManger.Pushs[tag].Enqueue(obj);
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


        #region 구조 경우에따라 사용할수도있는 함수 오버로딩
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
        #endregion

        /// <summary>
        /// 메인에서 배열을 통째로 던져주면, 팩토리가 알아서 순회하며 풀에 박아넣음
        /// </summary>
        public void SettingObject(int count, BaseObject[] targetArray)
        {
            if (targetArray == null || targetArray.Length == 0) return;

            for (int i = 0; i < targetArray.Length; i++)
            {
                // 1. 일반 객체 생성 (타워든 에너미든)
                string tagName = targetArray[i].gameObject.name;
                LoadDataCreatedObj<BaseObject>(tagName, targetArray[i]);

                // 2. 만약 이게 공격 타워라면? 탄(Bullet)까지 세트로 생성해라
                if (targetArray[i] is AttackTower attackTower)
                {
                    if (attackTower.Bullet != null)
                    {
                        string bulletTag = attackTower.Bullet.gameObject.name;
                        // 탄은 기본적으로 10개씩 넉넉히 (수치는 형님 마음대로)
                        for (int j = 0; j < 10; j++)
                        {
                            LoadDataCreatedObj<BaseObject>(bulletTag, attackTower.Bullet);
                        }
                        Debug.Log($"[Factory] {tagName}의 탄({bulletTag}) 풀링 완료");
                    }
                }
            }
        }
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }



    }//END Class
}
