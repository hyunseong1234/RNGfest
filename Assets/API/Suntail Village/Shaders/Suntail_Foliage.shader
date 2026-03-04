Shader "Raygeas/Suntail Foliage_URP"
{
    Properties
    {
        [Header(Maps)][Space(10)][MainTexture]_BaseMap("Albedo", 2D) = "white" {}
        _SmoothnessMap("Smoothness", 2D) = "white" {}
        
        [Header(Settings)][Space(5)]_BaseColor("Main Color", Color) = (1,1,1,1)
        _Smoothness("Smoothness Intensity", Range( 0 , 1)) = 0.5
        _Cutoff("Alpha Cutoff", Range( 0 , 1)) = 0.35
        
        [Header(Second Color Settings)][Space(5)][Toggle(_COLOR2ENABLE_ON)] _Color2Enable("Enable Second Color", Float) = 0
        _SecondColor("Second Color", Color) = (0,0,0,0)
        [KeywordEnum(World_Position, UV_Based)] _SecondColorOverlayType("Overlay Type", Float) = 0
        _SecondColorOffset("Offset", Float) = 0
        _SecondColorFade("Fade", Range( -1 , 1)) = 0.5
        _WorldScale("World Scale", Float) = 1
        
        [Header(Wind Settings)][Space(5)][Toggle(_ENABLEWIND_ON)] _EnableWind("Enable Wind", Float) = 1
        _WindForce("Wind Force", Range( 0 , 1)) = 0.3
        _WindWavesScale("Wind Waves Scale", Range( 0 , 1)) = 0.25
        _WindSpeed("Wind Speed", Range( 0 , 1)) = 0.5
        [Toggle(_ANCHORTHEFOLIAGEBASE_ON)] _Anchorthefoliagebase("Anchor Base (UV.y)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }
        Cull Off // 식생이므로 양면 렌더링

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _ENABLEWIND_ON
            #pragma shader_feature_local _ANCHORTHEFOLIAGEBASE_ON
            #pragma shader_feature_local _COLOR2ENABLE_ON
            #pragma shader_feature_local _SECONDCOLOROVERLAYTYPE_WORLD_POSITION _SECONDCOLOROVERLAYTYPE_UV_BASED

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Simple Noise Implementation
            float3 mod3D289(float3 x) { return x - floor(x / 289.0) * 289.0; }
            float4 mod3D289(float4 x) { return x - floor(x / 289.0) * 289.0; }
            float4 permute(float4 x) { return mod3D289((x * 34.0 + 1.0) * x); }
            float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - r * 0.85373472095314; }

            float snoise(float3 v)
            {
                const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);
                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + C.yyy;
                float3 x3 = x0 - 0.5;
                i = mod3D289(i);
                float4 p = permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0)) + i.y + float4(0.0, i1.y, i2.y, 1.0)) + i.x + float4(0.0, i1.x, i2.x, 1.0));
                float4 j = p - 49.0 * floor(p / 49.0);
                float4 x_ = floor(j / 7.0);
                float4 y_ = floor(j - 7.0 * x_);
                float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
                float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;
                float4 h = 1.0 - abs(x) - abs(y);
                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);
                float4 s0 = floor(b0) * 2.0 + 1.0;
                float4 s1 = floor(b1) * 2.0 + 1.0;
                float4 sh = -step(h, 0.0);
                float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
                float3 g0 = float3(a0.xy, h.x);
                float3 g1 = float3(a0.zw, h.y);
                float3 g2 = float3(a1.xy, h.z);
                float3 g3 = float3(a1.zw, h.w);
                float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
                g0 *= norm.x; g1 *= norm.y; g2 *= norm.z; g3 *= norm.w;
                float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m; m = m * m;
                return 42.0 * dot(m, float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3)));
            }

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float3 normalWS     : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SmoothnessMap);  SAMPLER(sampler_SmoothnessMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _SecondColor;
                float _Smoothness;
                float _Cutoff;
                float _SecondColorOffset;
                float _SecondColorFade;
                float _WorldScale;
                float _WindSpeed;
                float _WindWavesScale;
                float _WindForce;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                #ifdef _ENABLEWIND_ON
                    float mulTime = _Time.y * (_WindSpeed * 5.0);
                    float windNoise = snoise((worldPos + mulTime) * _WindWavesScale);
                    float windOffset = windNoise * 0.01;
                    
                    #ifdef _ANCHORTHEFOLIAGEBASE_ON
                        windOffset *= pow(input.uv.y, 2.0); // UV.y가 0인 바닥 고정
                    #endif
                    
                    input.positionOS.xyz += windOffset * (_WindForce * 30.0);
                #endif

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            void frag(Varyings input, out float4 outColor : SV_Target)
            {
                float4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Alpha Cutoff
                clip(albedoTex.a - _Cutoff);

                float3 finalAlbedo = albedoTex.rgb * _BaseColor.rgb;

                #ifdef _COLOR2ENABLE_ON
                    float maskSource = 0;
                    #if defined(_SECONDCOLOROVERLAYTYPE_WORLD_POSITION)
                        maskSource = snoise(input.worldPos * _WorldScale) * 0.5 + 0.5;
                    #else
                        maskSource = input.uv.y;
                    #endif
                    
                    float mask = saturate((maskSource + _SecondColorOffset) * (_SecondColorFade * 2.0));
                    finalAlbedo = lerp(finalAlbedo, albedoTex.rgb * _SecondColor.rgb, mask);
                #endif

                float smoothness = SAMPLE_TEXTURE2D(_SmoothnessMap, sampler_SmoothnessMap, input.uv).r * _Smoothness;

                // Lighting
                InputData inputData = (InputData)0;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(input.worldPos));
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.worldPos);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalAlbedo;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = smoothness;
                surfaceData.alpha = 1.0;

                outColor = UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // 그림자를 위한 Pass
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}