using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class ServiceLocator : MonoBehaviour
    {
        public static ServiceLocator Instance { get; private set; }

        private SerializedDictionary<Type, BaseManager> serviceRegistry = new SerializedDictionary<Type, BaseManager>();
        private List<UpdateManager> updateManagers = new List<UpdateManager>();

        private GameEventHandler _eventChain;

        public delegate void GameEventHandler(string eventData);

        public SerializedDictionary<Type, BaseManager> ServiceRegistry { get => serviceRegistry; set => serviceRegistry = value; }
        public List<UpdateManager> UpdateManagers { get => updateManagers; set => updateManagers = value; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitChildManagers();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }


        private void InitChildManagers()
        {
            // 자식들 중 BaseManager를 상속받은 모든 컴포넌트 가져오기
            var managers = GetComponentsInChildren<BaseManager>(true);

            foreach (var manager in managers)
            {
                RegisterService(manager);


            }

            Debug.Log($"[ServiceLocator] 총 {serviceRegistry.Count}개의 매니저 자동 등록 완료.");
        }
        public void RegisterService(BaseManager service)
        {
            if (service == null) return;

            // T가 아닌 실제 객체의 런타임 타입을 가져옴
            Type type = service.GetType();

            if (!serviceRegistry.ContainsKey(type))
            {
                serviceRegistry.Add(type, service);
                Debug.Log($"[ServiceLocator] Registered: {type.Name}");

                // 캐싱 로직
                if (service is UpdateManager updateManager)
                {
                    updateManagers.Add(updateManager);
                }
            }
        }

        public T GetService<T>() where T : BaseManager
        {
            Type key = typeof(T);
            if (serviceRegistry.TryGetValue(key, out BaseManager service))
            {
                return service as T;
            }

            Debug.LogError($"Service '{key}' 등록된 레지스트리 없음 순번확인이나 base AWake 받았는지 한번 ㄱㄱ");
            return null;
        }

        public void RegisterEventHandler(GameEventHandler handler)
        {
            _eventChain += handler;
            Debug.Log($"[ServiceLocator] 이벤트 핸들러 등록: {handler.Method.DeclaringType.Name}.{handler.Method.Name}");
        }

        public void TriggerEvent(string data)
        {
            _eventChain?.Invoke(data); // 등록된 모든 메서드 순차 호출
            Debug.Log("--- 이벤트 처리 완료---\n");
        }
    }
}

