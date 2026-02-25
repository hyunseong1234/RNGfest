using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Model
{
    public class TileObject : MonoBehaviour
    {
        /// <summary>
        /// 타워 존재 유무 판단 불 값
        /// </summary>
        public bool _isUsed = false;

        /// <summary>
        /// 자기 자신 위치
        /// </summary>
        public Vector3 _position;

        private void Awake()
        {
            //시작할때 초기화
            if (_position == Vector3.zero) _position = transform.position;
            if (_isUsed == true) _isUsed = false;
        }
    }
}