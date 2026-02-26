using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merge : MonoBehaviour
{
    public int unitLevel = 1;
    public GameObject nextLevelPrefab;

    [Header("분신 설정")]
    public GameObject ghostPrefab; // 드래그할 때 보여줄 투명한 분신 프리팹

    private GameObject currentGhost;
    private Vector3 originalPosition;
    private Plane dragPlane; // 드래그를 계산할 가상의 바닥

   

    private void OnMouseDown()
    {
        // 1. 마우스를 누르는 순간, 분신(Ghost)을 생성합니다.
        // 본체(this)는 그 자리에 그대로 있습니다.
        currentGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);

        // 2. 분신에게 내 정보를 전달합니다 (레벨 등)
        // 분신에도 MergeGhost 같은 스크립트가 있어야 합니다. (아래 참고)
        var ghostScript = currentGhost.GetComponent<MergeGhost>();
        ghostScript.Setup(this);

        // 3. 드래그 평면 설정
        dragPlane = new Plane(Vector3.up, transform.position);
    }

    private void OnMouseDrag()
    {
        if (currentGhost == null) return;

        // 평면 위에서 마우스 위치 추적 (화면 모서리에서도 정확함)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            currentGhost.transform.position = ray.GetPoint(distance);
        }
    }

    private void OnMouseUp()
    {
        if (currentGhost == null) return;

        // 5. 분신이 머지 대상을 찾았는지 확인합니다.
        currentGhost.GetComponent<MergeGhost>().CheckMerge();
    }


}
