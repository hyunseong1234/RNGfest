using Dev.cheol.Comon;
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
        [SerializeField] private BaseObject[] _prfavs_Ui = null;

        [SerializeField] private BaseObject[] _prefabs = null;
        private Dictionary<string, TowerData> _towerDataCache = new Dictionary<string, TowerData>();

        public BaseObject[] Prefabs_Twoer { get => _prefabs_Twoer; set => _prefabs_Twoer = value; }
        public BaseObject[] Prefabs_Enmey { get => _prefabs_Enmey; set => _prefabs_Enmey = value; }
        public BaseObject[] Prefabs_Bullet { get => _prefabs_Bullet; set => _prefabs_Bullet = value; }

        private void Awake()
        {
            StartSetting();

            // 공통 리소스 로드 (적, 투사체, UI는 기존대로)
            _prefabs_Enmey = Resources.LoadAll<BaseObject>("Prefabs/CYC/Enemy");
            _prefabs_Bullet = Resources.LoadAll<BaseObject>("Prefabs/CYC/Bullet");
            _prfavs_Ui = Resources.LoadAll<BaseObject>("Prefabs/CYC/UI/BaseUI");

            // 2. 타워 로드 (PlayFab 데이터 체크)
            var userData = PlayFabDataManager.Instance?.userData;

            if (userData != null && userData._towerSlots != null && userData._towerSlots.Count > userData._currentSlot)
            {
                Tower[] allTowerPrefabs = Resources.LoadAll<Tower>("Prefabs/CYC/Tower");

                var currentDeck = userData._towerSlots[userData._currentSlot].slotTowers;

                _prefabs_Twoer = allTowerPrefabs
                    .Where(prefab => currentDeck.Any(type => prefab.name.Contains(type.ToString())))
                    .ToArray();

                Debug.Log($"[Factory] 서버 슬롯({userData._currentSlot}) 필터링 완료: {_prefabs_Twoer.Length}개 타워 준비됨.");

                foreach (var p in _prefabs_Twoer) Debug.Log($"로드된 타워: {p.name}");
            }
            else
            {
                _prefabs_Twoer = Resources.LoadAll<Tower>("Prefabs/CYC/Tower");
                Debug.Log("[Factory] 서버 데이터 없음. 전체 타워 로드.");
            }

            // 3. 전체 프리팹 리스트 병합 및 캐싱
            _prefabs = _prefabs_Enmey.Concat(_prefabs_Twoer).Concat(_prefabs_Bullet).Concat(_prfavs_Ui).ToArray();

            // TowerData SO 캐싱 (기존 로직 유지)
            var allDatas = Resources.LoadAll<TowerData>("Data/Towers");
            foreach (var data in allDatas)
            {
                if (!_towerDataCache.ContainsKey(data.towerName))
                    _towerDataCache.Add(data.towerName, data);
            }
        }

        public TowerData GetTowerData(string towerName)
        {
            _towerDataCache.TryGetValue(towerName, out var data);
            return data;
        }

        private void Start()
        {
            SettingObject(8, _prefabs_Enmey);
            SettingObject(20, _prefabs_Twoer);
            SettingObject(50, _prfavs_Ui);

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

            // [수정 포인트] UI 여부에 따라 부모 설정 분기
            if (obj.transform is RectTransform)
            {
                obj.transform.SetParent(poolingManger.PushlTransformUI, false);
            }
            else
            {
                obj.transform.SetParent(poolingManger.PushlTransform);
            }

            poolingManger.Pushs[tag].Enqueue(obj);
        }

        public void LoadDataCreatedObj<T>(string tag, T path) where T : BaseObject
        {
            T obj = Instantiate(path);
            obj.gameObject.SetActive(false);
            obj.PoolTag = tag;

            if (!poolingManger.Pushs.ContainsKey(tag))
            {
                poolingManger.Pushs.Add(tag, new Queue<BaseObject>());
            }

            // [수정 포인트] UI 여부에 따라 부모 설정 분기
            if (obj.transform is RectTransform)
            {
                obj.transform.SetParent(poolingManger.PushlTransformUI, false);
            }
            else
            {
                obj.transform.SetParent(poolingManger.PushlTransform);
            }

            poolingManger.Pushs[tag].Enqueue(obj);
        }

        #region 구조 경우에따라 사용할수도있는 함수 오버로딩
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

        public void SettingObject(int count, BaseObject[] targetArray)
        {
            if (targetArray == null || targetArray.Length == 0) return;

            for (int i = 0; i < targetArray.Length; i++)
            {
                string tagName = targetArray[i].gameObject.name;

                // [형님, 여기입니다!] 불렛이 아닌 '데미지 폰트'가 50개 생성되려면 이 루프가 필수입니다.
                for (int j = 0; j < count; j++)
                {
                    // 여기서 루프를 돌아야 DamageFont가 50개 생성되어 풀로 들어갑니다.
                    LoadDataCreatedObj<BaseObject>(tagName, targetArray[i]);
                }

                // 기존 타워/불렛 로직 (건드릴 필요 없음)
                if (targetArray[i] is AttackTower attackTower)
                {
                    if (attackTower.Bullet != null)
                    {
                        string bulletTag = attackTower.Bullet.gameObject.name;
                        for (int k = 0; k < 10; k++)
                        {
                            LoadDataCreatedObj<BaseObject>(bulletTag, attackTower.Bullet);
                        }
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