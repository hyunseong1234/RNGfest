using Dev.cheol.Model;
using UnityEngine;

namespace Dev.jeon.Model
{
    public class PoisonBuff : BaseBuff
    {
        public int Damage { get; private set; }
        private float _tickInterval = 1.0f;
        private float _nextTick = 0f;

        public PoisonBuff(int damage)
        {
            Damage = damage;
            // 독 연출 위치만 따로 조정하고 싶다면 여기서 수정 가능
            _effectOffset = new Vector3(0, 1.5f, 0);
        }

        public void UpgradePoison(int newDamage)
        {
            Damage = newDamage;
        }

        protected override void OnStart()
        {
            _nextTick = _timer + _tickInterval;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_timer >= _nextTick)
            {
                if (_owner is Enemy enemy)
                {
                    enemy.OnDamaged(Damage, FontColor.Green);
                }
                _nextTick += _tickInterval;
            }
        }
    }
}