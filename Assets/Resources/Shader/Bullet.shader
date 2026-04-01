Shader "Custom/Particles/Unlit_ZTest" // 이름을 바꿔서 유니티 순정과 충돌 방지
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        
        // [추가] ZTest를 인스펙터에서 직접 조절할 수 있게 노출
        // 0:Disabled, 1:Never, 2:Less, 3:Equal, 4:LEqual, 5:Greater, 6:NotEqual, 7:GEqual, 8:Always
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest Mode", Float) = 4

        // ZWrite를 Off(0)로 설정하여 뒤에 그려지는 물체에 영향을 주지 않도록 합니다.
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite Mode", Float) = 0

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _BumpMap("Normal Map", 2D) = "bump" {}
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // ... (중간 파티클 속성 생략 - 기존과 동일) ...
        _SoftParticlesNearFadeDistance("Soft Particles Near Fade", Float) = 0.0
        _SoftParticlesFarFadeDistance("Soft Particles Far Fade", Float) = 1.0
        _CameraNearFadeDistance("Camera Near Fade", Float) = 1.0
        _CameraFarFadeDistance("Camera Far Fade", Float) = 2.0
        _DistortionBlend("Distortion Blend", Range(0.0, 1.0)) = 0.5
        _DistortionStrength("Distortion Strength", Float) = 1.0

        _Surface("__surface", Float) = 0.0
        _Blend("__mode", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _BlendOp("__blendop", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

    HLSLINCLUDE
    #pragma never_use_dxc
    ENDHLSL

    SubShader
    {
        Tags
        {
            //"RenderType" = "Opaque"
            "RenderType" = "Transparent" // 렌더 타입 수정
            "Queue" = "Transparent+100" // 불투명(2000)과 일반 투명(3000)보다 늦게 출력
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            
            // -------------------------------------
            // Render State Commands
            BlendOp[_BlendOp]
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            
            ZWrite[_ZWrite] // 프로퍼티의 _ZWrite 사용

            // [수정] 고정된 값이 아니라 프로퍼티에서 받은 _ZTest 값을 사용
            ZTest [_ZTest] 
            
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit
            
            // (키워드 및 인클루드 부분 생략 - 기존과 동일)
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _SOFTPARTICLES_ON
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitForwardPass.hlsl"
            ENDHLSL
        }
        
        // DepthOnly, SceneSelection 등 다른 Pass들도 동일하게 ZTest [_ZTest]를 넣어주면 좋습니다.
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    // CustomEditor를 제거하거나 주석 처리하여 유니티 UI가 값을 덮어쓰지 못하게 함
    // CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.ParticlesUnlitShader"
}