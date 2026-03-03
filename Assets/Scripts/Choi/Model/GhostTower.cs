using Dev.cheol.Model;
using System.Collections.Generic;
using UnityEngine;

public class GhostTower : MonoBehaviour
{
    public GameObject _ghostObject;
    private List<MeshFilter> _ghostMeshParts = new List<MeshFilter>();
    private List<Mesh> _bakedMeshes = new List<Mesh>();

    [SerializeField] private Material _hologramMaterial;

    // [추가] 고스트의 배율을 조절하고 싶다면 이 변수를 쓰세요.
    [SerializeField] private float _ghostScaleMultiplier = 1.0f;

    private void Awake()
    {
        if (_ghostObject == null) _ghostObject = this.gameObject;

        // 만약 프리팹 자체 스케일을 multiplier로 쓰고 싶다면 Awake에서 저장해둬도 됩니다.
    }

    public void ShowGhost(Tower targetTower)
    {
        _ghostObject.SetActive(true);

        var allSMRs = targetTower.GetComponentsInChildren<SkinnedMeshRenderer>();
        PrepareGhostParts(allSMRs.Length);

        for (int i = 0; i < allSMRs.Length; i++)
        {
            var smr = allSMRs[i];
            var targetFilter = _ghostMeshParts[i];
            var targetMesh = _bakedMeshes[i];

            smr.BakeMesh(targetMesh);
            targetFilter.sharedMesh = targetMesh;

            // 1. 자식 조각들의 위치와 회전은 맞추되, 스케일은 원본 비율을 따름
            targetFilter.transform.localPosition = smr.transform.localPosition;
            targetFilter.transform.localRotation = smr.transform.localRotation;
            targetFilter.transform.localScale = smr.transform.localScale; // 원본 조각 비율

            targetFilter.gameObject.SetActive(true);
        }

        for (int i = allSMRs.Length; i < _ghostMeshParts.Count; i++)
        {
            _ghostMeshParts[i].gameObject.SetActive(false);
        }

        // 전체 부모의 위치와 회전은 동기화하되, 스케일은 원본에 multiplier를 곱함
        // 만약 프리팹 스케일을 무조건 따르고 싶다면 이 줄을 주석 처리하거나 조절하세요.
        transform.position = targetTower.transform.position;
        transform.rotation = targetTower.transform.rotation * Quaternion.Euler(0, 180, 0);

        // 원본 타워 크기에 내가 지정한 배율을 곱함 (프리팹에서 1.2로 키웠다면 1.2f 입력)
        transform.localScale = targetTower.transform.localScale * _ghostScaleMultiplier;
    }

    private void PrepareGhostParts(int count)
    {
        while (_ghostMeshParts.Count < count)
        {
            GameObject part = new GameObject($"GhostPart_{_ghostMeshParts.Count}");
            part.transform.SetParent(_ghostObject.transform);

            // 레이어 설정
            part.layer = LayerMask.NameToLayer("Ignore Raycast");

            var filter = part.AddComponent<MeshFilter>();
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _hologramMaterial;

            Mesh mesh = new Mesh();
            _ghostMeshParts.Add(filter);
            _bakedMeshes.Add(mesh);
        }
    }

    public void HideGhost()
    {
        _ghostObject.SetActive(false);
    }
}