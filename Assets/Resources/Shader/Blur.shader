Shader "UI/URP/BackgroundBlur_Kawase"
{
    Properties
    {
        [Header(Blur Settings)]
        _BlurSize ("Blur Size (Radius)", Float) = 2.0
        _Iterations ("Iterations (Quality)", Range(1, 10)) = 5
        
        [Header(Overlay Settings)]
        _TintColor ("Tint Color", Color) = (0, 0, 0, 0.5)
        
        // UI 필수 속성들 (Stencil, ColorMask 등)
        [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline" // URP 전용 태그
        }

        // UI 필수 스텐실 설정
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIBackgroundBlur"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 grabPos    : TEXCOORD0; // 화면 좌표
                float4 color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // URP가 제공하는 배경 텍스처 변수명
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize; // 텍셀 크기 (1/위드, 1/하이트)

            float _BlurSize;
            int _Iterations;
            float4 _TintColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                // 화면 좌표 계산 (URP 방식)
                output.grabPos = ComputeScreenPos(output.positionCS);
                output.color = input.color;
                
                return output;
            }

            // Kawase Blur 샘플링 함수
            float4 KawaseBlur(TEXTURE2D_PARAM(tex, samplerTex), float2 uv, float2 texelSize, float pixelOffset)
            {
                float4 color = 0;
                // 4방향 대각선 샘플링
                color += SAMPLE_TEXTURE2D(tex, samplerTex, uv + float2(pixelOffset + 0.5, pixelOffset + 0.5) * texelSize);
                color += SAMPLE_TEXTURE2D(tex, samplerTex, uv + float2(-pixelOffset - 0.5, pixelOffset + 0.5) * texelSize);
                color += SAMPLE_TEXTURE2D(tex, samplerTex, uv + float2(-pixelOffset - 0.5, -pixelOffset - 0.5) * texelSize);
                color += SAMPLE_TEXTURE2D(tex, samplerTex, uv + float2(pixelOffset + 0.5, -pixelOffset - 0.5) * texelSize);
                return color * 0.25;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 화면 UV 계산
                float2 uv = input.grabPos.xy / input.grabPos.w;
                
                float4 blurColor = 0;
                float2 texelSize = _CameraOpaqueTexture_TexelSize.xy;

                // 반복 횟수(Iterations)만큼 샘플링 범위를 넓히며 블러 적용
                for (int j = 0; j < _Iterations; j++)
                {
                    // 각 반복마다 오프셋을 조절하여 더 넓게 퍼뜨림
                    float offset = (float)j * _BlurSize;
                    blurColor += KawaseBlur(TEXTURE2D_ARGS(_CameraOpaqueTexture, sampler_CameraOpaqueTexture), uv, texelSize, offset);
                }

                // 평균값 계산
                blurColor /= (float)_Iterations;

                // 배경 블러색과 Tint Color를 알파 블렌딩
                float4 finalColor = lerp(blurColor, _TintColor, _TintColor.a);
                
                // UI Masking 지원을 위한 알파 처리
                finalColor.a = input.color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}