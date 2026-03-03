Shader "Custom/URP_NeonFlowHologram"
{
    Properties
    {
        [HDR] _BaseColor("Hologram Color", Color) = (0, 1, 0.2, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        
        [Header(Neon Flow)]
        [HDR] _NeonColor("Neon Flow Color", Color) = (0, 1, 1, 1)
        _FlowSpeed("Flow Speed", Float) = 2.0      // 빛줄기가 흐르는 속도
        _FlowLength("Flow Length", Range(0.1, 5.0)) = 1.0  // 빛줄기의 길이
        _FlowDensity("Flow Density", Float) = 5.0   // 빛줄기 개수
        
        [Header(Glow Pulse)]
        _PulseSpeed("Pulse Speed", Float) = 1.0 
        _PulseIntensity("Pulse Intensity", Range(0, 1)) = 0.3 
        
        _Alpha("Alpha", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha One 
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _NeonColor;
                float _RimPower;
                float _FlowSpeed;
                float _FlowLength;
                float _FlowDensity;
                float _PulseSpeed;
                float _PulseIntensity;
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes input) {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                // 1. 기본 실루엣 (림라이트 베이스)
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normal = normalize(input.normalWS);
                float rimBase = 1.0 - saturate(dot(viewDir, normal));
                float rim = pow(rimBase, _RimPower);

                // 2. [핵심] 테두리를 타고 흐르는 빛줄기 (Neon Flow)
                // 월드 Y축과 시간(Time)을 조합해서 위아래로 흐르는 마스크 생성
                // frac을 사용하여 0~1 사이를 반복하게 만듦
                float flow = frac((input.positionWS.y * _FlowDensity) - (_Time.y * _FlowSpeed));
                
                // 빛줄기 형태를 날카롭게 깎음 (Pow를 높일수록 선이 얇아짐)
                float flowLine = pow(flow, 10.0 / _FlowLength);
                
                // 테두리(rim) 영역 내에서만 빛줄기가 나타나도록 마스킹
                float neonEffect = flowLine * rim * 15.0; // 쨍한 발광을 위해 강도 높임

                // 3. 전체적인 웅~우우우웅~ 깜빡임
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = saturate(pulse * _PulseIntensity + (1.0 - _PulseIntensity));

                // 4. 최종 색상 조합
                half4 finalColor = _BaseColor * rim; // 기본 홀로그램 실루엣
                finalColor.rgb += _NeonColor.rgb * neonEffect; // 흐르는 빛줄기 합치기
                
                finalColor.a = saturate(rim + neonEffect) * pulse * _Alpha;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}