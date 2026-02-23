using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dev.jeon.Object
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.zero;
        [SerializeField] private float moveSpeed = 5f;

        public float MoveSpeed => moveSpeed;

        private void Update()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        public void MoveTo(Vector3 drection)
        {
            moveDirection = drection;
        }

    }
}

