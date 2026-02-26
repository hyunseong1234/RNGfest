using Dev.cheol.Comon;
using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Enemy : BaseUnit
{
    private int _waypointIndex = 0; //현재 가고있는 플레그 인덱스
    [SerializeField] private int _getGold = 10; //몬스터가 사망시 주는돈

    protected override void Awake()
    {
        base.Awake();
    }


    /// <summary>
    /// 다음 경로를 찾는 함수
    /// </summary>
    public void RefreshPath()
    {
        var mapManager = ServiceLocator.Instance.GetService<MapManager>();
        if (mapManager == null) return;

        Transform[] path = mapManager.FlagPoints;

        // 인덱스 범위 확인
        if (path != null && _waypointIndex < path.Length)
        {
            Target = path[_waypointIndex];
            // 여기서 index를 올리지 않고, 목적지에 도달했을 때만 올리도록 ObjectUpdate와 맞춥니다.
        }
        else
        {
            Target = null;
            Debug.Log($"{this.name} 종점 도착");

            ServiceLocator.Instance.GetService<MainManager>().RemoveUnit(this);
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        _waypointIndex = 0;
    }




    public void OnDamaged(int damage)
    {

        _status.Hp -= damage; // StatusInfo에 hp가 있다고 가정

        if (_status.Hp <= 0)
        {
            var system = ServiceLocator.Instance.GetService<SystemManager>();
            //디지는 판정은 여기서 하기 때문에 돈주는거랑 더미 연출도 여기다가 넣을 예정
            ServiceLocator.Instance.GetService<MainManager>().RemoveUnit(this);
            system.Gold += _getGold;
        }
    }

    /// <summary>
    /// 업데이트 로직 
    /// </summary>
    public override void ObjectUpdate()
    {
        // 타겟이 없으면 새로 경로를 찾음
        if (Target == null)
        {
            RefreshPath();
            if (Target == null) return; // 종점이면 중단
        }

        // 유클리드 제곱 거리 계산
        float dx = Target.position.x - transform.position.x;
        float dz = Target.position.z - transform.position.z;
        float sqrDistanceXZ = (dx * dx) + (dz * dz);

        //도착 판정 (0.1f 거리)
        if (sqrDistanceXZ < 0.01f)
        {
            _waypointIndex++; // 다음 지점으로 인덱스 증가
            RefreshPath();    // 타겟 갱신
            return;
        }

        // 4. 상태 머신 연동
        if (Status.Speed > 0)
        {
            ChangeState(EState.MOVE);
        }
        else
        {
            ChangeState(EState.IDLE);
        }
    }

    public override void ActiveAttack()
    {
        throw new System.NotImplementedException();
    }
}




