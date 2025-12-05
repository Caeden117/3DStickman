Shader "Unlit/NodeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		MyColor ("Color Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		// Shared code between base and add passes
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		sampler2D _MainTex;
		float4x4 MyTRSMatrix;
		fixed4 MyColor;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
			float3 worldNormal : TEXCOORD1;
			float3 worldPos : TEXCOORD2;
			SHADOW_COORDS(3)
		};

		v2f vert (appdata v)
		{
			v2f o;
			
			// Apply custom transformation
			float4 worldVertex = mul(MyTRSMatrix, v.vertex);
			o.pos = mul(UNITY_MATRIX_VP, worldVertex);
			o.worldPos = worldVertex.xyz;
			
			// Transform normal to world space
			float3 worldNormal = normalize(mul((float3x3)MyTRSMatrix, v.normal));
			o.worldNormal = worldNormal;
			
			o.uv = v.uv;
			
			TRANSFER_SHADOW(o)
			
			return o;
		}
		ENDCG

		// Forward base pass - initial color and lighting
		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Sample the texture
				fixed4 albedo = tex2D(_MainTex, i.uv) * MyColor;
				
				// Normalize interpolated normal
				float3 worldNormal = normalize(i.worldNormal);
				float3 worldViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				
				// Ambient lighting
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo.rgb;
				
				// Directional light (main light)
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float NdotL = max(0, dot(worldNormal, lightDir));
				fixed3 diffuse = _LightColor0.rgb * albedo.rgb * NdotL;
				
				// Apply shadows
				fixed shadow = SHADOW_ATTENUATION(i);
				diffuse *= shadow;
				
				// Combine lighting
				fixed3 finalColor = ambient + diffuse;
				
				return fixed4(finalColor, albedo.a);
			}
			ENDCG
		}
		
		// Add pass for additional lights (point, spot, directional, etc.)
		Pass
		{
			Tags { "LightMode"="ForwardAdd" }
			Blend One One
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Sample the texture
				fixed4 albedo = tex2D(_MainTex, i.uv) * MyColor;
				
				// Normalize interpolated normal
				float3 worldNormal = normalize(i.worldNormal);
				float3 worldViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				
				// Light direction and attenuation
				float3 lightDir;
				fixed atten;
				
				#ifdef POINT
					lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
					atten = 1.0 / (1.0 + length(_WorldSpaceLightPos0.xyz - i.worldPos) * _WorldSpaceLightPos0.w);
				#elif DIRECTIONAL
					lightDir = normalize(_WorldSpaceLightPos0.xyz);
					atten = 1.0;
				#elif SPOT
					lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
					float distSqr = dot(_WorldSpaceLightPos0.xyz - i.worldPos, _WorldSpaceLightPos0.xyz - i.worldPos);
					atten = 1.0 / (1.0 + distSqr * _WorldSpaceLightPos0.w);
				#endif
				
				// Diffuse lighting
				float NdotL = max(0, dot(worldNormal, lightDir));
				fixed3 diffuse = _LightColor0.rgb * albedo.rgb * NdotL * atten;
				
				// Apply shadows
				fixed shadow = SHADOW_ATTENUATION(i);
				diffuse *= shadow;
				
				return fixed4(diffuse, 0);
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

