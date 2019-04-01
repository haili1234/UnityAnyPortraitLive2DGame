/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/


Shader "AnyPortrait/Editor/Colored Texture ToneColor (2X)"
{
	Properties
	{
		_Color("2X Tone Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Albedo (RGBA)", 2D) = "white" {}							// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)			// ScreenSize for clipping in Editor
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Add
		//Blend OneMinusDstColor One//Soft Add
		//Blend DstColor Zero//Multiply
		//Blend DstColor SrcColor//2X Multiply
		//Cull Off

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleColor alpha //<<AlphaBlend인 경우
		//#pragma surface surf SimpleColor//AlphaBlend가 아닌 경우

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		half4 LightingSimpleColor(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;

		}

		half4 _Color;
		sampler2D _MainTex;
		float4 _ScreenSize;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
			float4 screenPos;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV.y = 1.0f - screenUV.y;

			half4 c = tex2D(_MainTex, IN.uv_MainTex);

			half a_0 = tex2D(_MainTex, IN.uv_MainTex + float2(0.01f, 0.01f)).a;
			half a_1 = tex2D(_MainTex, IN.uv_MainTex + float2(-0.01f, 0.01f)).a;
			half a_2 = tex2D(_MainTex, IN.uv_MainTex + float2(0.01f, -0.01f)).a;
			half a_3 = tex2D(_MainTex, IN.uv_MainTex + float2(-0.01f, -0.01f)).a;

			half a_4 = tex2D(_MainTex, IN.uv_MainTex + float2(0.015f, 0)).a;
			half a_5 = tex2D(_MainTex, IN.uv_MainTex + float2(0, 0.015f)).a;
			half a_6 = tex2D(_MainTex, IN.uv_MainTex + float2(-0.015f, 0)).a;
			half a_7 = tex2D(_MainTex, IN.uv_MainTex + float2(0, -0.015f)).a;

			half outlineItp = 1 - ((a_0 + a_1 + a_2 + a_3 + a_4 + a_5 + a_6 + a_7) / 8.0f); // 0~1 => 0.2 ~ 1 
			outlineItp = (outlineItp * 0.8f) + 0.2f;

			c.rgb = c.r * 0.3f + c.g * 0.6f + c.b * 0.1f;//<<GrayScale
			c.rgb *= 2.0f;
			

			c.rgb *= _Color.rgb * 2.0f;
			//c.rgb *= IN.color;//<<이 색상은 적용하지 않습니다.
			
			o.Alpha = c.a * _Color.a * outlineItp;
			
			
			if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
			{
				o.Alpha = 0;
				discard;
			}
			if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
			{
				o.Alpha = 0;
				discard;
			}
			o.Albedo = c.rgb;
		}
		ENDCG
		}
		FallBack "Diffuse"
}
