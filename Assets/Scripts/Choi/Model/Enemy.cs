using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Manager;
using UnityEngine;

public class Enemy : BaseUnit
{
    public int _waypointIndex = 0; //현재 가고있는 플레그 인덱스
    [SerializeField] private int _getGold = 10; //몬스터가 사망시 주는돈

    protected float _currentShield = 0;
    protected BaseObject _shieldVisualInstance; // 생성된 보호막 인스턴스
    protected BaseObject _shatterPrefab;        // 깨질 때 쓸 프리팹
    public int GetGold { get => _getGold; set => _getGold = value; }
    public int BaseDamage { get; set; }
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

            ServiceLocator.Instance.GetService<WaveManager>().TakeDamage(BaseDamage);

            ServiceLocator.Instance.GetService<MainManager>().RemoveUnit(this);
        }
    }

    


    public void AddShield(float amount, BaseObject shieldPrefab, BaseObject shatterPrefab)
    {
        _currentShield += amount;
        _shatterPrefab = shatterPrefab; // 깨지는 연출 기억

        if (_shieldVisualInstance == null && shieldPrefab != null)
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            // 프리팹으로 풀링
            _shieldVisualInstance = pool.GetFromPool<BaseObject>(shieldPrefab);
            if (_shieldVisualInstance != null)
            {
                _shieldVisualInstance.gameObject.SetActive(true);
                _shieldVisualInstance.transform.SetParent(this.transform);
                _shieldVisualInstance.transform.localPosition = Vector3.up * 1.0f;
            }
        }
    }

    public void OnDamaged(float damage, FontColor colortype)
    {
        var main = ServiceLocator.Instance.GetService<MainManager>();
        var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

        // 보호막 선처리
        if (_currentShield > 0)
        {
            if (_currentShield >= damage)
            {
                _currentShield -= damage;
                damage = 0;
            }
            else
            {
                damage -= _currentShield;
                _currentShield = 0;
            }

            // 보호막 파괴 시점
            if (_currentShield <= 0) BreakShield(pool);
        }

        //  체력 처리
        if (damage > 0)
        {
            _stat.CurrentHp -= damage;

            var damageObj = pool.GetFromPool<DamageFont>("DamageFont");
            if (damageObj != null)
            {
                damageObj.SetDamage(damage, transform, colortype);
                main.SpawnUI.Add(damageObj);
            }
        }

        if (_stat.CurrentHp <= 0)
        {
            OnDeath();// 사망 시 호출될 가상 함수 추가

            var system = ServiceLocator.Instance.GetService<SystemManager>();
            main.RemoveUnit(this);
            system.Gold += _getGold;
        }
    }

    protected virtual void OnDeath() { }

    private void BreakShield(ObjectPoolingManger pool)
    {
       // Debug.Log($"<color=red><b>[보호막 파괴]</b> {gameObject.name}의 보호막이 완전히 파괴되었습니다!</color>");
        // 1. 입고 있던 보호막 반납
        if (_shieldVisualInstance != null)
        {
            pool.ReturnPool(_shieldVisualInstance);
            _shieldVisualInstance = null;
        }

        // 2. 쪼개지는 파티클 즉시 생성 (프리팹으로 풀링)
        if (_shatterPrefab != null)
        {
            var shatter = pool.GetFromPool<BaseObject>(_shatterPrefab);
            if (shatter != null)
            {
                shatter.gameObject.SetActive(true);
                shatter.transform.position = transform.position + Vector3.up * 1.0f;

            }
        }
    }

    /// <summary>
    /// 업데이트 로직 
    /// </summary>
    public override void ObjectUpdate()
    {
        base.ObjectUpdate();
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
        if (_stat.Speed.Value > 0)
        {
            ChangeState(EState.MOVE);
        }
        else
        {
            ChangeState(EState.IDLE);
        }
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        _waypointIndex = 0;
        _stat.CurrentHp = _stat.MaxHp.Value;

        // 반납 시 보호막 제거
        if (_shieldVisualInstance != null)
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(_shieldVisualInstance);
            _shieldVisualInstance = null;
        }
        _stat.CurrentHp = _stat.MaxHp.Value;
    }
    private void OnReachEndPath()
    {
        // 1. WaveManager에 데미지 전달
        ServiceLocator.Instance.GetService<WaveManager>().TakeDamage(BaseDamage);

        // 2. 몬스터 관리 리스트에서 제거 (WaitUntil 통과를 위해 필수)
        ServiceLocator.Instance.GetService<MainManager>().SpawnEnemys.Remove(this);

        // 3. 풀로 반환
        gameObject.SetActive(false); // 또는 Pool 반환 함수 호출
    }

    public override void ActiveAttack()
    {
        throw new System.NotImplementedException();
    }
}




