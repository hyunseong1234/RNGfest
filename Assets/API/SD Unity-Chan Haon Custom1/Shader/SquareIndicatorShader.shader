Shader "Custom/SquareIndicator"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (MainText)", 2D) = "white" {}
		_Angle("Angle", Range(0,360)) = 120.0
		_FillNum("Filled Num",Range(0,1))=0.5
		_FillColor("Filled Color", Color) = (1,1,1,1)
		_FillTex("_FillTex", 2D) = "white" {}
	}
		SubShader
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Transparent"
			}
			CGPROGRAM
			#pragma surface surf Lambert alpha:fade

			sampler2D _MainTex;
			sampler2D _FillTex;
			half _Angle;
			half4 _Color;
			float _FillNum;
			half4 _FillColor;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_FillTex;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				float4 c = tex2D(_MainTex, IN.uv_MainTex);
				//float4 e = tex2D(_FillTex, IN.uv_FillTex);
				float4 e = tex2D(_FillTex, float2(IN.uv_FillTex.x, IN.uv_FillTex.y - _Time.y));

				float2 pos = IN.uv_MainTex;

				float4 finalTex;
				half4 finalColor;

				if (pos.y <= _FillNum)
				{
					finalColor = _FillColor;
					finalTex = e;
				}
				else
				{
					finalColor = _Color;
					finalTex = c;
				}

				o.Albedo = finalTex.rgb * finalColor;
				o.Alpha = c.a * finalColor.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}