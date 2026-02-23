using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]private Transform[] waypoints;
    [SerializeField]private float moveSpeed = 5f;

    private int currentWayPointIndex = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWayPointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position ,
            targetWaypoint.position,
            moveSpeed * Time.deltaTime);  

        if(Vector3.Distance(transform.position, targetWaypoint.position )< 0.1f)
        {
            currentWayPointIndex++; 

            if(currentWayPointIndex >= waypoints.Length)
            {
                ReachDestination(); // 목적지에 도달
            }
        }
    }

    private void ReachDestination()
    {
        Debug.Log("적이 목적지에 도착");

        // 적 삭제 나중에 오브젝트 풀링 넣기
        Destroy(gameObject);
    }
}
