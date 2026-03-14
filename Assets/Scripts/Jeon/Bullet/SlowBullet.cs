using Dev.cheol.Model;
using Dev.jeon.Model;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SlowBullet : BaseBullet
{
    [SerializeField] private float _slowAmount = 0.5f;
    [SerializeField] private float _slowDuration = 2.0f;

    protected override void ApplyHitLogic(Vector3 hitPoint) //  Vector3 인자 추가
    {
        if (_target != null && _target.TryGetComponent(out Enemy enemy))
        {
            enemy.OnDamaged(_damage, _fontColor);

            var existingSlow = enemy.GetBuff<SlowBuff>();
            if (existingSlow == null)
            {
                var slowBuff = new SlowBuff(_slowAmount);
                slowBuff.Init(enemy, _slowDuration, _hitEffectPrefab);
                enemy.AddBuff(slowBuff);
            }
            else existingSlow.Refresh(_slowDuration);
        }
    }
}