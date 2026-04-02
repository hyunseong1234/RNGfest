
using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Collections.Generic;

namespace Dev.jeon.Buff
{
    public class BuffHandler
    {
        private BaseUnit _owner;
        private List<BaseBuff> _buffs = new List<BaseBuff>();

        public BuffHandler(BaseUnit owner)
        {
            _owner = owner;
        }

        public void AddBuff(BaseBuff newBuff, float duration, BaseObject effectPrefab)
        {
            // 동일한 타입의 버퍼가 있는지 확인 (중첩/갱신 로직)
            var existingBuff = _buffs.Find(b => b.GetType() == newBuff.GetType());
            if (existingBuff != null)
            {
                existingBuff.Refresh(duration);
            }
            else
            {
                newBuff.Init(_owner, duration, effectPrefab);
                _buffs.Add(newBuff);
            }
        }

        public void HandleUpdate(float deltaTime)
        {
            if (_buffs.Count == 0) return;

            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                _buffs[i].BuffUpdate(deltaTime);
                if (_buffs[i].IsFinished)
                {
                    _buffs.RemoveAt(i);
                }
            }
        }
        public T GetBuff<T>() where T : BaseBuff
        {
            for (int i = 0; i < _buffs.Count; i++)
            {
                if (_buffs[i] is T typedBuff) return typedBuff;
            }
            return null;
        }

        public void ClearAll()
        {
            foreach (var buff in _buffs) buff.EndBuff();
            _buffs.Clear();
        }
    }
}

