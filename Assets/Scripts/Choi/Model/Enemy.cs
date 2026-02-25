using Dev.cheol.Comon;
using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Enemy : BaseUnit
{
    private int _waypointIndex = 0;

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

            OnDead();
        }
    }

    /// <summary>
    /// 몬스터 리턴풀 처리 외부호출 있을 필요있으니 퍼블릭으로함
    /// </summary>
    public void OnDead()
    {
        // 풀링 반납 및 정리
        ServiceLocator.Instance.GetService<MainManager>().SpawnEnemys.Remove(this); //1
        OnReturnToPool(); //2
        ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this); //3
    }

    /// <summary>
    /// 객체가 사라질때 
    /// </summary>
    public void OnReturnToPool()
    {

        StopAllCoroutines();
        currentStateCoroutine = null;

        _waypointIndex = 0;
        Target = null;

        ChangeState(EState.IDLE);
    }

    public void OnDamaged(int damage)
    {

        _status.Hp -= damage; // StatusInfo에 hp가 있다고 가정

        if (_status.Hp <= 0)
        {
            OnDead(); // 만들어둔 3종 세트 호출
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


}




