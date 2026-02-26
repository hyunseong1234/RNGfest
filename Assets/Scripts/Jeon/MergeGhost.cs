using UnityEngine;

public class MergeGhost : MonoBehaviour
{
    private Merge parentTower; // 나를 만든 본체 타워

    public void Setup(Merge parent)
    {
        parentTower = parent;
    }

    public void CheckMerge()
    {
        // 주변에 머지할 수 있는 '본체' 타워가 있는지 찾습니다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f);
        Merge targetTower = null;

        foreach (var col in colliders)
        {
            var other = col.GetComponent<Merge>();
            // 본체(parentTower)가 아니고, 레벨이 같은 타워 찾기
            if (other != null && other != parentTower && other.unitLevel == parentTower.unitLevel)
            {
                targetTower = other;
                break;
            }
        }

        if (targetTower != null)
        {
            // [머지 성공]
            Vector3 spawnPos = targetTower.transform.position;
            Instantiate(parentTower.nextLevelPrefab, spawnPos, Quaternion.identity);

            Destroy(targetTower.gameObject); // 상대방 타워 파괴
            Destroy(parentTower.gameObject); // 나의 본체 타워 파괴
        }

        // 머지 성공 여부와 상관없이 분신은 삭제
        Destroy(gameObject);
    }
}