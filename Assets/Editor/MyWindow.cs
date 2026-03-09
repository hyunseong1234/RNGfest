using UnityEditor;
using UnityEngine;

public class MyWindow : EditorWindow
{
    [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<MyWindow>();
        // 탭 이름과 아이콘을 변경합니다.
        window.titleContent = new GUIContent("새로운 탭 이름");
    }
}
