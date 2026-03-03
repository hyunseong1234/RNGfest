using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace Dev.cheol.UI
{
    public class LankStarUI : ScreenWorldUI
    {
        [SerializeField] private BaseUnit _unit;
        [SerializeField] private List<RankUIChild> _rankObj;


        /// <summary>
        /// UI초기 세팅값
        /// </summary>
        /// <param name="tower"></param>
        public void Init(Tower tower)
        {

            _target = null; //혹시나 버그있을까봐 한번 초기화 하고 
            _target = tower.transform;

            _rankObj.ForEach(t => t.gameObject.SetActive(false));

            _rankObj[tower.Lank - 1].gameObject.SetActive(true);
            tower.StarUI = this; //스파게티 완성 직전 Good!!! 객체간의 첫 커플링 발생!!

        }

        private void OnDisable()
        {
            _rankObj.ForEach(a => a.gameObject.SetActive(false));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 1. 리스트 자체가 메모리에 없으면(Null) 새로 만들어줍니다.
            if (_rankObj == null)
            {
                _rankObj = new List<RankUIChild>();
            }

            // 2. 프리팹 내부 요소들이 중복으로 담기지 않게 싹 비워줍니다.
            _rankObj.Clear();

            // 3. 자식들을 찾아 리스트에 채워넣습니다.
            var children = this.GetComponentsInChildren<RankUIChild>(true); // (true)를 넣으면 꺼져있는 자식도 찾습니다.
            if (children != null && children.Length > 0)
            {
                _rankObj.AddRange(children);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}에 RankUIChild 자식이 하나도 없누!");
            }
        }
    }


}
