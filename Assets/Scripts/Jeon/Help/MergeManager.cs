using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dev.Help
{
    public class MergeManager : UpdateManager
    {
        #region Fields
        private Tower _draggingUnit = null;
        private Vector3 _originalPosition;
        [SerializeField] private LayerMask _targetLayer; // Tower와 Tile 레이어 포함
        #endregion

        public override void HandleEvent(string data) => throw new System.NotImplementedException();

        public override void ManagerUpdate()
        {
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            // 1. 클릭 시작 (Pick Up)
            if (Input.GetMouseButtonDown(0))
            {
                AttemptPickUp();
            }

            // 2. 드래그 중 (Dragging)
            if (Input.GetMouseButton(0) && _draggingUnit != null)
            {
                UpdateDragging();
            }

            // 3. 드래그 종료 (Drop)
            if (Input.GetMouseButtonUp(0) && _draggingUnit != null)
            {
                AttemptDrop();
            }
        }

        private void AttemptPickUp()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _targetLayer))
            {
                var unit = hit.collider.GetComponent<Tower>();
                if (unit != null)
                {
                    _draggingUnit = unit;
                    _originalPosition = unit.transform.position;

                    // 드래그 시 레이캐스트 방해 금지 (임시 레이어 변경 혹은 콜라이더 비활성화)
                    _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
            }
        }

        private void UpdateDragging()
        {
            // 마우스 위치를 월드 좌표로 변환 (바닥 평면 기준)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, _originalPosition); // 유닛이 서있던 높이 기준

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPos = ray.GetPoint(distance);
                // 유닛이 공중에 살짝 떠 있는 연출을 위해 Y값 조절 가능
                _draggingUnit.transform.position = targetPos + Vector3.up * 0.2f;
            }
        }

        private void AttemptDrop()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Tower"); // 레이어 복구

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _targetLayer))
            {
                var targetTower = hit.collider.GetComponent<Tower>();

                // 합성 조건 체크 (가까운 거리 + 같은 종류 등)
                if (targetTower != null && targetTower != _draggingUnit)
                {
                    // 유클리드 거리 체크 (이미 레이캐스트로 잡았지만 한 번 더 검증)
                    float sqrDist = (targetTower.transform.position - _originalPosition).sqrMagnitude;

                    if (CanMerge(_draggingUnit, targetTower))
                    {
                        // 합성 성공
                        ExecuteMerge(_draggingUnit, targetTower);
                        return;
                    }
                }
            }

            // 합성 실패 시 원래 위치로 복귀
            _draggingUnit.transform.position = _originalPosition;
            _draggingUnit = null;
        }

        private bool CanMerge(Tower origin, Tower target)
        {
            return origin.PoolTag == target.PoolTag && origin.Lank == target.Lank;
        }

        private void ExecuteMerge(Tower origin, Tower target)
        {
            var pooling = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var main = ServiceLocator.Instance.GetService<MainManager>();
            main.RemoveUnit(origin);
            main.RemoveUnit(target);
            Debug.Log($"{origin.name}와 {target.name} 합성 실행!");
            _draggingUnit = null;
        }
    }
}