using UnityEngine;

public class MergeGhost : MonoBehaviour
{
    private Merge parentTower;
    private float checkRadius;
    public void Setup(Merge parent, float radius)
    {
        parentTower = parent;
        checkRadius = radius;
    }
    public void CheckMerge()
    {
        // 하드코딩된 1.5f 대신 본체에서 정해준 checkRadius 사용
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius);
        Merge targetTower = null;

        foreach (var col in colliders)
        {
            // GetComponent 대신 안전하고 빠른 TryGetComponent 사용
            if (col.TryGetComponent<Merge>(out var other))
            {
                // 본체가 아니고, 레벨이 같은 타워인지 확인
                if (other != parentTower && other.unitLevel == parentTower.unitLevel)
                {
                    targetTower = other;
                    break;
                }
            }
        }

        // targetTower를 찾았고, 합쳐질 다음 레벨의 프리팹이 등록되어 있을 때만 실행 (에러 방지)
        if (targetTower != null && parentTower.nextLevelPrefab != null)
        {
            // [머지 성공]
            Vector3 spawnPos = targetTower.transform.position;
            Instantiate(parentTower.nextLevelPrefab, spawnPos, Quaternion.identity);

            Destroy(targetTower.gameObject);
            Destroy(parentTower.gameObject);
        }

        Destroy(gameObject); // 분신은 항상 마지막에 삭제
    }
}