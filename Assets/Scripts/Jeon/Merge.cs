using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merge : MonoBehaviour
{
    public int unitLevel = 1;
    public GameObject nextLevelPrefab;

    [Header("분신 설정")]
    public GameObject ghostPrefab; // 드래그할 때 보여줄 투명한 분신 프리팹

    [Header("머지 설정")]
    public float minDragDistance = 0.1f;

    private MergeGhost currentGhostScript;
    private Plane dragPlane;

    private Vector3 dragOffset;

    private void OnMouseDown()
    {
        GameObject ghostObj = Instantiate(ghostPrefab, transform.position, transform.rotation);
        currentGhostScript = ghostObj.GetComponent<MergeGhost>();
        currentGhostScript.Setup(this, minDragDistance);

        dragPlane = new Plane(Vector3.up, transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            // 내 위치에서 마우스가 찍힌 바닥 위치를 빼서 차이를 구함
            dragOffset = transform.position - hitPoint;
        }
    }

    private void OnMouseDrag()
    {
        if (currentGhostScript == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            currentGhostScript.transform.position = ray.GetPoint(distance);
        }
    }
    private void OnMouseUp()
    {
        if (currentGhostScript == null) return;

        // 1. 본체(나)와 분신(이동한 위치) 사이의 거리를 계산합니다.
        float dragDistance = Vector3.Distance(transform.position, currentGhostScript.transform.position);

        // 2. 드래그한 거리가 최소 거리(minDragDistance)보다 짧다면 단순 클릭으로 간주!
        if (dragDistance < minDragDistance)
        {
            // 머지를 진행하지 않고 분신만 파괴합니다.
            Destroy(currentGhostScript.gameObject);
        }
        else
        {
            // 3. 충분히 드래그를 했다면 정상적으로 머지 판정을 진행합니다.
            currentGhostScript.CheckMerge();
        }

        currentGhostScript = null; // 참조 초기화
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDragDistance);
    }


}
