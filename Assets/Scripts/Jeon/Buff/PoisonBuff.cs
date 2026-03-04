using Dev.cheol.Model;
using Dev.jeon.Bullet;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Dev.jeon.Model
{
    public class PoisonBuff : BaseBuff
    {
        public int Damage { get; private set; }
        private float _tickInterval = 1.0f;
        private float _nextTick = 0f;

        public PoisonBuff(int damage) // 생성자로 데이터 받기
        {
            Damage = damage;

            //    _effectPrefabName = "PoisonEffect";
            //    _effectOffset = new Vector3(0, 1.2f, 0);
        }

        public void UpgradePoison(int newDamage)
        {
            Damage = newDamage;
            Debug.Log($"<color=yellow>[독 강화]</color> 더 강력한 맹독({Damage})으로 갱신되었습니다!"); // 갱신 로그도 추가해두면 좋습니다
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
                if (_owner is Enemy enemy)
                {
                    enemy.OnDamaged(Damage, FontColor.Green);

                    Debug.Log($"<color=green>[독 도트 피해]</color> 틱 데미지 {Damage}이(가) 들어갔습니다!");
                }
                _nextTick += _tickInterval;
            }
        }
    }
}