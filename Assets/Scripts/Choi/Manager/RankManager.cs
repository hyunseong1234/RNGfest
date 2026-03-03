using Dev.cheol.Model;
using Dev.cheol.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


namespace Dev.cheol.Manager
{

    public class RankManager : BaseManager
    {
        [SerializeField] private Transform rankUIParent; //안넣어져있으면 파인드 강제로 도니깐 절때로 링크 필수
        [SerializeField] private List<LankStarUI> _lankUIs = new List<LankStarUI>();
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

        private void OnEnable()
        {
            if (rankUIParent == null)
            {
                FindParent();
            }
            else
            {
                var ranks = rankUIParent.GetComponentsInChildren<LankStarUI>().ToList();
                _lankUIs.AddRange(ranks);
            }

        }

        /// <summary>
        /// 성급 박으려면 외부에서 이거 호출해주면됨
        /// </summary>
        /// <param name="tower"></param>
        public void RequestRank(Tower tower)
        {
            var useList = _lankUIs.Where(a => a.Target == null).FirstOrDefault();
            if (useList.gameObject.activeSelf == false) useList.gameObject.SetActive(true);
            useList.Init(tower);
        }



        /// <summary>
        /// 어쩔수 없이 객체 찾이용
        /// </summary>
        private void FindParent()
        {
            if (rankUIParent == null)
                rankUIParent = GameObject.Find(" RankUIParent").transform;

            Debug.LogError("파인드 사용 " + rankUIParent + "가 널값 RankManager 참고");
            OnEnable();
        }


    }

}
