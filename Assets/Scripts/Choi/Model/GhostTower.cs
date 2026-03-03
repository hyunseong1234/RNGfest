using Dev.cheol.Model;
using System.Collections.Generic;
using UnityEngine;

public class GhostTower : MonoBehaviour
{
    public GameObject _ghostObject;

    // 자식 메쉬 조각들을 관리할 리스트 (메모리 재사용용)
    private List<MeshFilter> _ghostMeshParts = new List<MeshFilter>();
    private List<Mesh> _bakedMeshes = new List<Mesh>();

    [SerializeField] private Material _hologramMaterial; // 인스펙터에서 홀로그램 머티리얼 넣어주세요

    private void Awake()
    {
        if (_ghostObject == null) _ghostObject = this.gameObject;
    }

    public void ShowGhost(Tower targetTower)
    {
        _ghostObject.SetActive(true);

        // 1. 타겟 타워의 모든 SkinnedMeshRenderer를 다 긁어모읍니다.
        var allSMRs = targetTower.GetComponentsInChildren<SkinnedMeshRenderer>();

        // 2. 고스트 자식 개수가 모자라면 더 만듭니다. (오브젝트 풀링 방식)
        PrepareGhostParts(allSMRs.Length);

        // 3. 각 조각별로 메쉬 구워서 박기
        for (int i = 0; i < allSMRs.Length; i++)
        {
            var smr = allSMRs[i];
            var targetFilter = _ghostMeshParts[i];
            var targetMesh = _bakedMeshes[i];

            // 현재 포즈 굽기
            smr.BakeMesh(targetMesh);
            targetFilter.sharedMesh = targetMesh;

            // 원본 조각의 상대적 위치/회전/스케일 동기화
            targetFilter.transform.localPosition = smr.transform.localPosition;
            targetFilter.transform.localRotation = smr.transform.localRotation;
            targetFilter.transform.localScale = smr.transform.localScale;

            targetFilter.gameObject.SetActive(true);
        }

        // 사용 안 하는 남는 자식들은 끄기
        for (int i = allSMRs.Length; i < _ghostMeshParts.Count; i++)
        {
            _ghostMeshParts[i].gameObject.SetActive(false);
        }

        // 4. 전체 위치 동기화
        transform.position = targetTower.transform.position;
        transform.rotation = targetTower.transform.rotation;
        transform.localScale = targetTower.transform.localScale;
    }

    private void PrepareGhostParts(int count)
    {
        while (_ghostMeshParts.Count < count)
        {
            GameObject part = new GameObject($"GhostPart_{_ghostMeshParts.Count}");
            part.transform.SetParent(_ghostObject.transform);

            var filter = part.AddComponent<MeshFilter>();
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _hologramMaterial;

            Mesh mesh = new Mesh();
            mesh.name = $"GhostMesh_{_ghostMeshParts.Count}";

            _ghostMeshParts.Add(filter);
            _bakedMeshes.Add(mesh);
        }
    }

    public void HideGhost()
    {
        _ghostObject.SetActive(false);
    }
}