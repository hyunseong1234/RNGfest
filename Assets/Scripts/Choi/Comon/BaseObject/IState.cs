using Dev.cheol.Model;
using System.Collections;
using UnityEngine;
namespace Dev.cheol.Comon
{

    /// <summary>
    /// 상태 추상화
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        /// <param name="unit"></param>
        IEnumerator Enter(BaseUnit unit);
        /// <summary>
        /// 매 프레임마다 호출
        /// </summary>
        /// <param name="unit"></param>
        IEnumerator Execute(BaseUnit unit);
        /// <summary>
        /// 상태 종료 시 호출
        /// </summary>
        /// <param name="unit"></param>
        IEnumerator Exit(BaseUnit unit);
    }
}

