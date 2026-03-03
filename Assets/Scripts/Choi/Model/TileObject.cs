using UnityEngine;

namespace Dev.cheol.Model
{
    public class TileObject : MonoBehaviour
    {
        public bool _isUsed = false;
        public Vector3 _position;

        private Renderer _renderer;
        private Color _originColor;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null) _originColor = _renderer.material.color;

            if (_position == Vector3.zero) _position = transform.position;
            _isUsed = false;
        }

        /// <summary>
        /// 타일의 하이라이트 상태를 변경합니다.
        /// </summary>
        /// <param name="state">0: 기본, 1: 가능(파랑), 2: 불가능(빨강) 3: 자기 자신</param>
        public void SetHighlight(int state)
        {
            if (_renderer == null) return;

            switch (state)
            {
                case 1: // 머지 가능 (파란색)
                    _renderer.material.color = new Color(0, 0.5f, 1f, 0.5f);
                    break;
                case 2: // 머지 불가 (빨간색)
                    _renderer.material.color = new Color(1f, 0, 0, 0.5f);
                    break;
                case 3: // 현재 내 위치 (노란색)
                    _renderer.material.color = new Color(1f, 0.92f, 0.016f, 0.5f); // 쨍한 노랑
                    break;
                default: // 기본 복구
                    _renderer.material.color = _originColor;
                    break;
            }
        }
    }
}