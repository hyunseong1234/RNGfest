using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.Object
{
    public class Enemy : MonoBehaviour
    {
        private int waypointCount;
        private Transform[] wayPoints;
        private int currentIndex = 0;
        private Movement movement;

        public void Setup(Transform[] wayPoints)
        {
            movement = GetComponent<Movement>();

            waypointCount = wayPoints.Length;
            this.wayPoints = wayPoints;

            transform.position = wayPoints[currentIndex].position;

            StartCoroutine("OnMove");
        }

        private IEnumerator OnMove()
        {
            NextMoveTo();
            
            while(true)
            {

                if(Vector3.Distance(transform.position, wayPoints[currentIndex].position) < 0.02f * movement.MoveSpeed)
                {
                    NextMoveTo();
                }
                yield return null;
            }
            
        }

        private void NextMoveTo()
        {
            if(currentIndex < waypointCount -1)
            {
                transform.position = wayPoints[currentIndex].position;
                currentIndex++;
                Vector3 direction = (wayPoints[currentIndex].position - transform.position).normalized;
                movement.MoveTo(direction);
            }
            else
            {
                Destroy(gameObject);
            }
        }
            
    }

}