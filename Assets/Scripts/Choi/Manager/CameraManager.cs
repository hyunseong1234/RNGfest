using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class CameraManager : BaseManager
    {
        [SerializeField] private Camera _camera = null;

        public Camera Camera { get => _camera; set => _camera = value; }

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }
    }

}
