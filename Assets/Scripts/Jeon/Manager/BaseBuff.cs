using Dev.cheol.Model; // BaseUnit이 있는 네임스페이스
using UnityEngine;

namespace Dev.jeon.Model // 유저님의 아키텍처에 맞춘 네임스페이스
{
    // [System.Serializable]을 붙이면 유니티 인스펙터창에서 BaseUnit을 클릭했을 때
    // 현재 무슨 버프가 몇 초 남았는지 디버깅하기 아주 좋습니다.
    [System.Serializable]
    public abstract class BaseBuff
    {
        protected BaseUnit _owner; // 버프가 걸려있는 주인 (적 or 아군)
        protected float _duration; // 버프 총 지속 시간
        protected float _timer;    // 현재까지 흐른 시간

        // 이 버프가 끝났는지 체크하는 플래그 (BaseUnit이 이걸 보고 리스트에서 지웁니다)
        public bool IsFinished { get; private set; } = false;

        /// <summary>
        /// 버프가 처음 대상에게 부여될 때 1회 실행됩니다.
        /// </summary>
        public virtual void Init(BaseUnit owner, float duration)
        {
            _owner = owner;
            _duration = duration;
            _timer = 0f;
            IsFinished = false;

            OnStart(); // 자식 클래스에서 스탯 증가 등을 구현할 곳
        }

        /// <summary>
        /// 매니저 -> BaseUnit -> BuffUpdate 순서로 매 프레임 들어옵니다.
        /// </summary>
        public void BuffUpdate(float deltaTime)
        {
            if (IsFinished) return;

            _timer += deltaTime;
            OnUpdate(deltaTime); // 자식 클래스에서 도트 데미지 등을 구현할 곳

            // 수명이 다하면 스스로 종료를 선언합니다.
            if (_timer >= _duration)
            {
                EndBuff();
            }
        }

        /// <summary>
        /// 이미 걸린 버프를 또 맞았을 때, 시간만 다시 덮어씌웁니다 (갱신).
        /// </summary>
        public virtual void Refresh(float newDuration)
        {
            _duration = newDuration; // 시간 갱신 (예: 다시 5초로)
            _timer = 0f;             // 흐른 시간 0초로 리셋
            IsFinished = false;
        }

        /// <summary>
        /// 버프가 끝날 때 (시간 종료 or 유닛 사망으로 인한 강제 해제) 호출됩니다.
        /// </summary>
        public void EndBuff()
        {
            if (IsFinished) return;
            IsFinished = true;
            OnEnd(); // 자식 클래스에서 스탯 원상복구 등을 구현할 곳
        }

        // =========================================================
        // 아래 3개는 자식 클래스(PoisonBuff 등)가 입맛에 맞게 채워 넣을 빈 공간입니다.
        // =========================================================
        protected virtual void OnStart() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnEnd() { }
    }
}