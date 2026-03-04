using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Model
{
    public class SlowBuff : BaseBuff
    {
        private float _slowAmount;   // 감속 비율
        private float _originalSpeed; // 원래 속도 저장용

        public SlowBuff(float slowAmount)
        {
            _slowAmount = slowAmount;
        }

        protected override void OnStart()
        {
            if (_owner != null)
            {
                // 1. 원본 Status를 로컬 변수에 복사
                var tempStatus = _owner.Status;

                // 2. 원래 속도 저장 및 로컬 변수 값 수정
                _originalSpeed = tempStatus.Speed;
                tempStatus.Speed = _originalSpeed * _slowAmount;

                // 3. 수정된 복사본을 다시 원본에 덮어씌움
                _owner.Status = tempStatus;

                Debug.Log($"<color=blue>[Slow]</color> 속도 감소: {_originalSpeed} -> {_owner.Status.Speed}");
            }
        }

        protected override void OnEnd()
        {
            if (_owner != null)
            {
                // 1. 원본 Status를 로컬 변수에 복사
                var tempStatus = _owner.Status;

                // 2. 원래 속도로 값 수정
                tempStatus.Speed = _originalSpeed;

                // 3. 수정된 복사본을 다시 원본에 덮어씌움
                _owner.Status = tempStatus;

                Debug.Log("<color=blue>[Slow]</color> 속도 복구됨");
            }
        }
    }
}