// UTS2GUI_URP.cs - Optimized for Unity 2022.3 LTS (URP)
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityChan
{
    public class UTS2GUI : ShaderGUI
    {
        // URP 환경에서는 렌더링 큐와 소트 레이어를 명시적으로 관리하는 것이 좋습니다.
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            Material material = materialEditor.target as Material;

            // 1. URP 대응 필수 체크: 가끔 하드웨어 가속 문제로 깨지는 현상 방지
            if (material.shader.name.Contains("Universal Render Pipeline") || material.shader.name.Contains("Toon"))
            {
                // 속성 찾기
                MaterialProperty mainTex = FindProperty("_MainTex", props, false);
                MaterialProperty baseColor = FindProperty("_BaseColor", props, false);

                EditorGUI.BeginChangeCheck();
                {
                    // 헤더 영역
                    EditorGUILayout.LabelField("UTS2 URP Setup (Unity 2022.3)", EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    // 기본 텍스처 설정
                    if (mainTex != null)
                    {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Base Map (URP)"), mainTex, baseColor);
                    }

                    // 2. 중요: URP용 렌더링 옵션 강제 활성화
                    if (material.HasProperty("_Cull"))
                    {
                        materialEditor.ShaderProperty(FindProperty("_Cull", props), "Culling Mode");
                    }

                    // 기존 UTS 속성들 나열
                    materialEditor.PropertiesDefaultGUI(props);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    // URP는 키워드 갱신이 엄격합니다.
                    foreach (var obj in materialEditor.targets)
                        MaterialChanged((Material)obj);
                }
            }
        }

        // URP 메테리얼 키워드 및 패스 업데이트
        static void MaterialChanged(Material material)
        {
            if (material == null) return;

            // 렌더 큐 설정 (URP 표준 준수)
            if (material.HasProperty("_Surface") && material.GetFloat("_Surface") == 0) // Opaque
            {
                material.renderQueue = (int)RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
            }
            else // Transparent
            {
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetOverrideTag("RenderType", "Transparent");
            }
        }
    }
}