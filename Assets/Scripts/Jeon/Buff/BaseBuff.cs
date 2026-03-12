using Dev.cheol.Manager;
using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Model
{
    [System.Serializable]
    public abstract class BaseBuff
    {
        protected BaseUnit _owner;
        protected float _duration;
        protected float _timer;

        // 이펙트 인스턴스와 프리팹 참조 저장
        protected BaseObject _effectInstance;
        protected BaseObject _effectPrefab;
        protected Vector3 _effectOffset = new Vector3(0, 1.5f, 0);

        public bool IsFinished { get; private set; } = false;

        // Init 함수에서 BaseObject 프리팹을 인자로 받습니다
        public virtual void Init(BaseUnit owner, float duration, BaseObject effectPrefab)
        {
            _owner = owner;
            _duration = duration;
            _effectPrefab = effectPrefab; // 전달받은 프리팹 저장
            _timer = 0f;
            IsFinished = false;

            SpawnEffect();
            OnStart();
        }
        // 몬스터가 다시 맞았을 때 호출되는 함수
        public virtual void Refresh(float newDuration)
        {
            _duration = newDuration;
            _timer = 0f;
            IsFinished = false;
        }
        private void SpawnEffect()
        {
            // 프리팹이 할당되어 있지 않으면 생성하지 않음
            if (_effectPrefab == null) return;

            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (pool == null) return;

            // 이름(문자열) 대신 프리팹 참조를 사용하여 풀에서 꺼냅니다
            _effectInstance = pool.GetFromPool<BaseObject>(_effectPrefab);

            if (_effectInstance != null)
            {
                _effectInstance.gameObject.SetActive(true);
                _effectInstance.transform.SetParent(_owner.transform);
                _effectInstance.transform.localPosition = _effectOffset;
            }
        }

        public void BuffUpdate(float deltaTime)
        {
            if (IsFinished) return;
            _timer += deltaTime;
            OnUpdate(deltaTime);
            if (_timer >= _duration) EndBuff();
        }

        public void EndBuff()
        {
            if (IsFinished) return;
            IsFinished = true;
            RemoveEffect();
            OnEnd();
        }

        private void RemoveEffect()
        {
            if (_effectInstance != null)
            {
                var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
                if (pool != null) pool.ReturnPool(_effectInstance);
                _effectInstance = null;
            }
        }

        protected virtual void OnStart() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnEnd() { }
    }
}