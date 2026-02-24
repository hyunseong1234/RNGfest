using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merge : MonoBehaviour
{
    public int unitLevel = 1;
    public GameObject nextLevelPrefab;

    private Vector3 offset;
    private float mZCoord;
    private Vector3 originalPosition;
    private bool isDragging = false;

    private void OnMouseDown()
    {
        originalPosition = transform.position;
        isDragging = true;

        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        offset = gameObject.transform.position - GetMouseWorldPos();

        transform.position += Vector3.up * 0.5f;
    }

    private void OnMouseDrag()
    {
        Vector3 newPos = GetMouseWorldPos() + offset;

        transform.position  = newPos;   
    }

    private void OnMouseUp()
    {
        isDragging = false;
        CheckForMerge();
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    private void CheckForMerge()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.0f);

        Merge closestTarget = null;
        float minDist = float.MaxValue;

        foreach(Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            Merge otherUnit = col.GetComponent<Merge>();

            if(otherUnit != null && otherUnit.unitLevel == this.unitLevel)
            {
                float dist = Vector3.Distance(transform.position, otherUnit.transform.position);

                if(dist < minDist)
                {
                    minDist = dist;
                    closestTarget = otherUnit;
                }
            }
        }
        if(closestTarget != null )
        {
            MergeWith(closestTarget);
        }
        else
        {
            transform.position = originalPosition;
        }
    }
    private void MergeWith(Merge other)
    {
        Vector3 spawnPos = (transform.position + other.transform.position) / 2;

        if (nextLevelPrefab != null)
        {
            Instantiate(nextLevelPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.Log($"Level {unitLevel + 1} 캐릭터가 없어서 생성 못함!");
        }

        Destroy(other.gameObject);
        Destroy(gameObject);


    }


}
