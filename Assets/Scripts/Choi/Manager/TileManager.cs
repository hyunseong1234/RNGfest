using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dev.cheol.Manager
{
    public class TileManager : BaseManager
    {

        [SerializeField] private Transform _pathTilesParrent = null;
        [SerializeField] private Transform _mapTilesParrent = null;

        /// <summary>
        /// 경로관련 타일들
        /// </summary>
        [SerializeField] private TileObject[] _pathTiles = null;
        /// <summary>
        /// 타워배치 관련 타일들
        /// </summary>
        [SerializeField] private TileObject[] _mapTile = null;

        /// <summary>
        /// 경로관련 타일들
        /// </summary>
        public TileObject[] PathTiles { get => _pathTiles; set => _pathTiles = value; }

        /// <summary>
        /// 타워배치 관련 타일들
        /// </summary>
        public TileObject[] MapTile { get => _mapTile; set => _mapTile = value; }

        private void Awake()
        {
            _pathTilesParrent = FindNameTr(_pathTilesParrent, "FlagPoint");
            _mapTilesParrent = FindNameTr(_mapTilesParrent, "TowerBuildArea");

            _pathTiles = _pathTilesParrent.GetComponentsInChildren<Model.TileObject>();
            _mapTile = _mapTilesParrent.GetComponentsInChildren<Model.TileObject>();

        }
        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// 파인드함수 죄악의 함수 쓰면 디진다. 앵간하면 안돌리게끔해라
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Transform FindNameTr(Transform tr, string name)
        {
            if (tr != null) return tr;
            Debug.Log("Find는 악마의 함수 링킹을 안한것을 감지하였습니다. 경고 디지기싫으면 링크를 하도록");
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                tr = go.transform;
            }
            else
            {
                Debug.LogWarning($"[MapManager] {name} 오브젝트를 씬에서 찾을 수 없습니다.");
            }

            return tr;
        }
    }


}
