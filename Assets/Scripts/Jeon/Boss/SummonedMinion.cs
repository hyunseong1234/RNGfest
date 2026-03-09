using Dev.cheol.Model;
using Dev.cheol.Manager;
using UnityEngine;

namespace Dev.jeon.Boss
{
    // Enemy를 상속받아 기본 이동/데미지 기능을 그대로 씁니다.
    public class SummonedMinion : Enemy
    {
        public void SetupMinion(float hp, int waypointIndex)
        {
            // 1. 능력치 설정 (BaseValue 수정)
            if (_stat != null)
            {
                _stat.MaxHp.BaseValue = hp;
                _stat.CurrentHp = hp;
            }

            // 2. 경로 정보 설정
            _waypointIndex = waypointIndex;
            Target = null; // 초기화

            // 3. 스스로 뇌 가동 (뇌 가동 로직을 내부에 숨김)
            RefreshPath();

            // 4. 상태 전환 (내 스스로 이동 시작)
            if (_stat.Speed.Value > 0)
            {
                ChangeState(EState.MOVE);
            }

            // 5. 매니저 리스트 등록 (나 태어났어요!)
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main != null && !main.SpawnEnemys.Contains(this))
            {
                main.SpawnEnemys.Add(this);
            }
        }

        // 소환수는 죽을 때 골드를 안 주게 오버라이드 할 수도 있습니다.
        // public override void OnDamaged(...) { ... } // 필요시 수정
    }
}