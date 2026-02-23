using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class MainManager : BaseManager
    {
        [Header("Object Lists")]

        [SerializeField] private Transform environmentRoot;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints; // 인스펙터에서 할당 확인 필수!




        private void Start()
        {

            var factory = ServiceLocator.Instance.GetService<FactoryManager>();
            if (factory != null) factory.StartSetting();

          

            StartCoroutine(WaitForSeedAndSpawn());
        }

        private System.Collections.IEnumerator WaitForSeedAndSpawn()
        {

            yield return new WaitForSeconds(0.2f);

          
        }

        private void Update()
        {
            if (ServiceLocator.Instance.UpdateManagers == null) return;

            foreach (var manager in ServiceLocator.Instance.UpdateManagers)
            {
                manager.ManagerUpdate();
            }

        }

      
        public override void HandleEvent(string data) { }
    }
}