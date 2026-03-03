using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Help
{
    public class MergeManager : UpdateManager
    {
        #region Fields
        [SerializeField] private Tower _draggingUnit = null; // 선택된 원본 타워
        private Vector3 _originalPosition;
        [SerializeField] private LayerMask _targetLayer;

        [SerializeField] private GhostTower _ghost; // 홀로그램 제어 객체
        #endregion

        public override void HandleEvent(string data) => throw new System.NotImplementedException();

        public override void ManagerUpdate()
        {
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) AttemptPickUp();

            // 드래그 중
            if (Input.GetMouseButton(0) && _draggingUnit != null) UpdateDragging();

            // 드래그 종료
            if (Input.GetMouseButtonUp(0) && _draggingUnit != null) AttemptDrop();
        }

        private void AttemptPickUp()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _targetLayer))
            {
                var unit = hit.collider.GetComponent<Tower>();
                ServiceLocator.Instance.GetService<MainManager>().Selected = unit;

                if (unit != null)
                {
                    _draggingUnit = unit;
                    _originalPosition = unit.transform.position;

                    // 1. [고스트 생성] 원본은 가만히 두고 고스트를 띄웁니다.
                    _ghost.ShowGhost(unit);

                    // 2. [시각화] 형님이 만든 전체 타일 하이라이트 켜기
                    TileVisual();

                    // 원본 타워가 마우스 레이캐스트를 가리지 않게 레이어 변경
                    _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
            }
        }

        private void UpdateDragging()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, _originalPosition);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPos = ray.GetPoint(distance);

                // 3. [고스트 이동] 원본(_draggingUnit) 대신 고스트가 마우스를 따라갑니다.
                // 유닛이 공중에 살짝 떠 있는 연출
                _ghost.transform.position = targetPos + Vector3.up * 0.2f;
            }
        }

        private void AttemptDrop()
        {
            // 4. [고스트 숨기기] 드롭했으니 고스트는 퇴장
            _ghost.HideGhost();

            _draggingUnit.gameObject.layer = LayerMask.NameToLayer("Tower");

            var main = ServiceLocator.Instance.GetService<MainManager>();
            Tower closestTarget = null;
            float minSqrDist = 1.0f;

            // 5. [판정 기준 변경] 원본 위치가 아니라 '고스트의 현재 위치'를 기준으로 타겟을 찾습니다.
            Vector3 dropPos = _ghost.transform.position;

            foreach (var target in main.SpawnTowers)
            {
                if (target == _draggingUnit || target == null) continue;

                // 고스트 좌표와 필드 타워들 간의 거리 계산
                float sqrDist = (target.transform.position - dropPos).sqrMagnitude;

                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    closestTarget = target;
                }
            }

            var mapTiles = ServiceLocator.Instance.GetService<TileManager>().MapTile.ToList();

            if (closestTarget != null && CanMerge(_draggingUnit, closestTarget))
            {
                Debug.Log("고스트 드롭 기반 합성 성공!");
                main.Selected = null;
                ExecuteMerge(_draggingUnit, closestTarget);
                mapTiles.ForEach(a => a.SetHighlight(0));
                return;
            }

            // 실패 시 원본은 제자리(어차피 움직이지도 않았지만 확행), 하이라이트 끄기
            main.Selected = null;
            mapTiles.ForEach(a => a.SetHighlight(0));

            _draggingUnit.transform.position = _originalPosition;
            _draggingUnit = null;
        }

        private bool CanMerge(Tower origin, Tower target)
        {
            return origin.PoolTag == target.PoolTag && origin.Lank == target.Lank;
        }

        private void TileVisual()
        {
            MainManager main = ServiceLocator.Instance.GetService<MainManager>();

            foreach (var tower in main.SpawnTowers)
            {
                // 1. 만약 지금 내가 들고 있는 타워라면? 노란색으로 표시!
                if (tower == _draggingUnit)
                {
                    tower.CurrentTile.SetHighlight(3); // 3번 처리 (Yellow)
                    continue;
                }

                // 2. 나머지는 기존처럼 파랑/빨강 판단
                bool canMerge = CanMerge(_draggingUnit, tower);
                tower.CurrentTile.SetHighlight(canMerge ? 1 : 2);
            }
        }

        private void ExecuteMerge(Tower origin, Tower target)
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            int nextLank = origin.Lank + 1;

            if (!CanMergeDefault(nextLank, 7)) return;

            TileObject tempTile = target.CurrentTile;
            main.RemoveUnit(origin);
            main.RemoveUnit(target);

            main.BuildTower(tempTile, nextLank);

            Debug.Log($"{origin.name} 합성 완료!");
            _draggingUnit = null;
        }

        private bool CanMergeDefault(int nextLank, int maxLank)
        {
            if (nextLank > maxLank) return false;
            return true;
        }
    }
}