using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

namespace Dev.Help
{
    public class MergeManager : UpdateManager
    {
        #region Fields
        [SerializeField] private Tower _draggingUnit = null;
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
                ServiceLocator.Instance.GetService<MainManager>().Selected = unit;
                Debug.Log(hit.collider.name);
                if (unit != null)
                {
                    _draggingUnit = unit;
                    _originalPosition = unit.transform.position;

                    // 드래그 시 레이캐스트 방해 금지 (임시 레이어 변경 혹은 콜라이더 비활성화)
                    _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    TileVisual();
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

        /// <summary>
        /// 드롭 놨을때 돌아가는 함수
        /// </summary>
        private void AttemptDrop()
        {
            // 레이어 복구는 일단 해줌 (드래그 시작 시 Ignore로 바꿨을 경우)
            _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Tower");

            // 1. 모든 타워 리스트 가져오기 (MainManager에 저장된 리스트 활용)
            var main = ServiceLocator.Instance.GetService<MainManager>();
            Tower closestTarget = null;
            float minSqrDist = 1.0f; // 합성을 판정할 임계 거리 (예: 1칸 거리의 제곱)

            foreach (var target in main.SpawnTowers)
            {
                // 나 자신은 제외
                if (target == _draggingUnit || target == null) continue;

                // 2. 놓은 위치와 필드 타워들 간의 유클리드 제곱 거리 계산
                float sqrDist = (target.transform.position - _draggingUnit.transform.position).sqrMagnitude;

                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    closestTarget = target;
                }
            }
            var mapTiles = ServiceLocator.Instance.GetService<TileManager>().MapTile.ToList();
            // 3. 가장 가까운 타워가 있고 합성 조건이 맞으면 실행
            if (closestTarget != null && CanMerge(_draggingUnit, closestTarget))
            {
                Debug.Log("거리 기반 합성 성공!");
                main.Selected = null;
                ExecuteMerge(_draggingUnit, closestTarget);
                mapTiles.ForEach(a => a.SetHighlight(0)); //모든타일 기본값 복원
                return;
            }
            main.Selected = null;
            mapTiles.ForEach(a => a.SetHighlight(0)); //모든타일 기본값 복원
            // 4. 실패 시 복귀
            _draggingUnit.transform.position = _originalPosition;
            _draggingUnit = null;

        }

        /// <summary>
        /// 동일 태그 동일 랭크가 맞는지 확인 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool CanMerge(Tower origin, Tower target)
        {
            return origin.PoolTag == target.PoolTag && origin.Lank == target.Lank;
        }

        private void TileVisual()
        {
            string tag = _draggingUnit.PoolTag;
            MainManager main = ServiceLocator.Instance.GetService<MainManager>();

            foreach (var tower in main.SpawnTowers)
            {
                tower.CurrentTile.SetHighlight(CanMerge(_draggingUnit, tower) ? 1 : 2);
            }
        }

        /// <summary>
        /// 거리 감지 최종 확인후 타워 합성하는 부분
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        private void ExecuteMerge(Tower origin, Tower target)
        {
            var pooling = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var main = ServiceLocator.Instance.GetService<MainManager>();

            int currentLank = origin.Lank;
            int nextLank = currentLank + 1;

            //if () 현성아 여기 보라 슬라임전용 로직도 추가해주라
            //TODO : 각각의 타워의 (특수타워) 별떨어뜨리기나 확률 따지는 등등8성 초과 이런거 들어갈때 여기다가 넣으면되유 조상님
            if (!CanMergeDefault(nextLank, 7)) return;

            TileObject tempTile = target.CurrentTile;
            main.RemoveUnit(origin);
            main.RemoveUnit(target);

            ServiceLocator.Instance.GetService<MainManager>().BuildTower(tempTile, nextLank);

            Debug.Log($"{origin.name}와 {target.name} 합성 실행!");
            _draggingUnit = null;
        }

        /// <summary>
        /// 단순 맥스비교 불값 함수
        /// </summary>
        /// <param name="nextLank"></param>
        /// <param name="maxLank"></param>
        /// <returns></returns>
        private bool CanMergeDefault(int nextLank, int maxLank)
        {
            if (nextLank > maxLank)
            {
                Debug.Log("최대 성급에 도달하여 더 이상 합성할 수 없습니다.");
                // 실패 처리 (원래 위치로 복귀 등)
                return false;
            }

            return true;
        }
    }
}