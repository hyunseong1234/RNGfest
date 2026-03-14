using Dev.cheol.Model;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NormalBullet : BaseBullet
{
    protected override void ApplyHitLogic(Vector3 hitPoint) //  Vector3 인자 추가
    {
        if (_target != null && _target.TryGetComponent(out Enemy enemy))
        {
            enemy.OnDamaged(_damage, _fontColor);
        }
    }
}
