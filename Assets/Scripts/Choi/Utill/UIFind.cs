using System.Linq;
using UnityEngine;

public static class UIFind
{
    /// <summary>
    /// 씬에 비활성화된 객체까지 포함하여 특정 타입의 UI를 찾아옵니다. 
    /// </summary>
    /// <typeparam name="T">찾으려는 컴포넌트 타입</typeparam>
    /// <param name="uiReference">참조를 담을 변수</param>
    /// <returns>찾기 성공 여부</returns>
    public static bool TryGetOrFindUI<T>(ref T uiReference) where T : UnityEngine.Object
    {
        if (uiReference != null) return true;

        // 2. 씬 내에서 비활성화된 객체까지 포함하여 타입 탐색
        uiReference = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();

        if (uiReference != null)
        {
            Debug.Log($"<color=cyan>[Find]</color> {typeof(T).Name}를 새로 찾아 연결했습니다.");
            return true;
        }

        Debug.LogWarning($"<color=red>[Fail]</color> 씬에서 {typeof(T).Name} 타입을 찾을 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 객체의 타입이 여러개이면서 이름으로 찾아야될 경우
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uiReference"></param>
    /// <param name="objName"></param>
    /// <returns></returns>
    public static bool TryGetOrFindUI<T>(ref T uiReference, string objName) where T : UnityEngine.Object
    {
        // 1. 이미 참조가 있으면 통과
        if (uiReference != null) return true;

        // 2. 씬의 모든 T 타입 중 '이름'까지 일치하는 녀석을 찾음
        uiReference = Resources.FindObjectsOfTypeAll<T>()
            .FirstOrDefault(obj => obj.name == objName);

        if (uiReference != null)
        {
            Debug.Log($"<color=cyan>[Find]</color> '{objName}' ({typeof(T).Name})를 찾아 연결했습니다.");
            return true;
        }

        Debug.LogWarning($"<color=red>[Fail]</color> 씬에서 '{objName}' 이름을 가진 {typeof(T).Name}를 찾을 수 없습니다.");
        return false;
    }
}
