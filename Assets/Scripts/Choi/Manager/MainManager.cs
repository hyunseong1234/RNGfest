using Dev.cheol.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dev.cheol.Manager
{
    public class MainManager : BaseManager
    {
        [Header("Object Lists")]

        [SerializeField] private Transform environmentRoot;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints; // 인스펙터에서 할당 확인 필수!

        List<Enemy> _spawnEnemys; //기본적으로 사용되는 에너미들 객체용
        List<Tower> _spawnTowers; // 플레이어들의 타워

        private void Start()
        {

            var factory = ServiceLocator.Instance.GetService<FactoryManager>();
            if (factory != null) factory.StartSetting();

            StartCoroutine(WaitForSeedAndSpawn());
        }


        /// <summary>
        /// 초반 세팅용 코루틴 함수
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator WaitForSeedAndSpawn()
        {

            yield return new WaitForSeconds(0.2f);


        }

        private void Update()
        {
            if (ServiceLocator.Instance.UpdateManagers == null) return;
            if (ServiceLocator.Instance.UpdateManagers.Count <= 0) return;

            foreach (var manager in ServiceLocator.Instance.UpdateManagers)
            {
                manager.ManagerUpdate();
            }

            //업데이트
            UpdateList(_spawnEnemys);
            UpdateList(_spawnTowers);
        }

        private void UpdateList<T>(List<T> ts) where T : BaseObject
        {

            if (ts == null || ts.Count <= 0) return;
            for (int i = ts.Count - 1; i >= 0; i--)
            {
                if (ts[i] == null) //그럴리가 없으나 혹시라도 순회돌 대상이 리무브만되고 리스트값이 있는경우가 있을때
                {
                    ts.RemoveAt(i); // 예외처리
                    continue;
                }

                ts[i].ObjectUpdate();
            }

        }
        public override void HandleEvent(string data) { }
    }
}