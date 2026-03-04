Shader "Raygeas/Suntail Surface_URP"
{
    Properties
    {
        [Header(Maps)][Space(10)][MainTexture]_BaseMap("Albedo", 2D) = "white" {}
        [Normal]_NormalMap("Normal", 2D) = "bump" {}
        _MetallicGlossMap("Metallic/Smoothness", 2D) = "white" {}
        [HDR]_EmissionMap("Emission", 2D) = "white" {}
        
        [Header(Settings)][Space(5)]_BaseColor("Color", Color) = (1,1,1,1)
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
        _NormalScale("Normal Scale", Float) = 1
        _Metallic("Metallic Intensity", Range( 0 , 1)) = 1
        _Smoothness("Smoothness Intensity", Range( 0 , 1)) = 1
        
        [KeywordEnum(Metallic_Alpha, Albedo_Alpha)] _SmoothnessSource("Smoothness Source", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SMOOTHNESSSOURCE_METALLIC_ALPHA _SMOOTHNESSSOURCE_ALBEDO_ALPHA
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD3;
                float3 tangentWS    : TEXCOORD4;
                float3 bitangentWS  : TEXCOORD5;
                float3 viewDirWS    : TEXCOORD6;
            };

            TEXTURE2D(_BaseMap);           SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);         SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MetallicGlossMap);  SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_EmissionMap);       SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EmissionColor;
                float _NormalScale;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                return output;
            }

            void frag(Varyings input, out float4 outColor : SV_Target)
            {
                // Albedo
                float4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float3 albedo = albedoTex.rgb * _BaseColor.rgb;

                // Normal
                float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
                float3 normalWS = UnpackNormalScale(normalSample, _NormalScale);
                
                // Reconstruct Tangent Space Normal
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 worldNormal = normalize(mul(normalWS, tangentToWorld));

                // Metallic & Smoothness
                float4 specTex = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv);
                float metallic = specTex.r * _Metallic;
                
                float smoothnessBase = 0;
                #if defined(_SMOOTHNESSSOURCE_ALBEDO_ALPHA)
                    smoothnessBase = albedoTex.a;
                #else
                    smoothnessBase = specTex.a;
                #endif
                float smoothness = smoothnessBase * _Smoothness;

                // Emission
                float3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;

                // PBR Lighting Data
                InputData inputData = (InputData)0;
                inputData.normalWS = worldNormal;
                inputData.viewDirectionWS = normalize(input.viewDirWS);
                inputData.bakedGI = SampleSH(worldNormal);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.specular = float3(0, 0, 0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalWS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;

                outColor = UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
        
        // Shadow Caster & Meta Pass (생략 가능하나 그림자 등을 위해 필요함)
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}