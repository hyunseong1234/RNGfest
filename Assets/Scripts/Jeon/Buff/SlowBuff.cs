using Dev.cheol.Model;
using Dev.cheol.Stats;
using UnityEngine;

namespace Dev.jeon.Model
{
    public class SlowBuff : BaseBuff
    {
        private float _slowAmount;      // 감속 비율 (예: 0.3f 면 30% 감소)
        private StatModifier _modifier; // 생성된 수정치를 저장해뒀다가 나중에 제거함

        public SlowBuff(float slowAmount)
        {
            // 기획에 따라 다르지만, 보통 30% 감소면 -0.3f를 넘기거나 
            // 0.3f를 넘기고 내부에서 음수로 바꿉니다.
            _slowAmount = -Mathf.Abs(slowAmount);
        }

        protected override void OnStart()
        {
            if (_owner != null)
            {
                // 1. 수정치 데이터 생성 (퍼센트 방식)
                // Source에 this를 넣어서 이 버프가 준 값임을 명시합니다.
                _modifier = new StatModifier(_slowAmount, StatModType.Percent, this);

                // 2. 주인의 스탯에서 Speed를 찾아 Modifier 추가
                // GetUnitStats()는 BaseUnitStats를 반환하므로 바로 Speed에 접근 가능
                _owner._stat.Speed.AddModifier(_modifier);

                Debug.Log($"<color=blue>[Slow]</color> 버프 시작: {_slowAmount * 100}% 감소");
            }
        }

        protected override void OnEnd()
        {
            if (_owner != null && _modifier != null)
            {
                // 3. 버프가 끝나면 내가 넣었던 Modifier만 쏙 제거
                // 이러면 다른 버프가 걸려있어도 내 지분만 빠지므로 수치가 안전함
                _owner._stat.Speed.RemoveModifier(_modifier);

                Debug.Log("<color=blue>[Slow]</color> 버프 종료: 속도 복구됨");
            }
        }
    }
}