using Dev.cheol.Model;
using Dev.jeon.Bullet;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Dev.jeon.Model
{
    public class PoisonBuff : BaseBuff
    {
        private int _damage;
        private float _tickInterval = 1.0f;
        private float _nextTick = 0f;

        public PoisonBuff(int damage) // 생성자로 데이터 받기
        {
            _damage = damage;
        }

        protected override void OnStart()
        {
            // 시작하자마자 첫 틱은 1초 뒤로 설정
            _nextTick = _timer + _tickInterval;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_timer >= _nextTick)
            {
                // 주인(Owner)이 Enemy라면 데미지 입힘
                if (_owner is Enemy enemy)
                {
                    enemy.OnDamaged(_damage);
                    Debug.Log($"<color=green>[독 도트 피해]</color> 앗 따가워! 틱 데미지 {_damage}이(가) 들어갔습니다!");

                }

                _nextTick += _tickInterval; // 다음 틱 설정
            }
        }
    }
}