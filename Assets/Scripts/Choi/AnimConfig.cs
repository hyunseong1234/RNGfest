using Dev.cheol.Model;
using System;
using UnityEngine;


public enum AniModel
{
    None = 0,
    ATK1,
    ATK2,
    ATK3,
    ATK4,
    Tired,
    MAX = 9999
}

[Serializable]
public struct AnimData
{

    public AniModel eAniName; // 열거형 종류

    // 런타임에서 사용할 실제 해시값 (인스펙터 노출 X)
    private int _aniHash;
    public int AniHash => _aniHash;
    public void Init()
    {
        _aniHash = Animator.StringToHash(eAniName.ToString());
    }
}
[CreateAssetMenu]
public class AnimConfig : ScriptableObject
{
    public AnimData[] datas;
}