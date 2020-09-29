// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "xray"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_Normal("Normal", 2D) = "bump" {}
		_Color("See-through color",Color) = (0,0,0,0)
		[HideInInspector] _texcoord("", 2D) = "white" {}
		[HideInInspector] __dirty("", Int) = 1
	}
		SubShader
		{

			Tags{ "Queue" = "Overlay-1" }

				ZWrite Off
				Cull Off
				ZTest Always
				CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			half filler;
		};

		uniform float4 _Color;

		inline half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
			return half4 (0, 0, 0, s.Alpha);
		}

		void surf(Input i , inout SurfaceOutput o)
		{
			o.Emission = _Color.rgb;
		}

		ENDCG

			Cull Back
			ZWrite On
			ZTest Less
			CGPROGRAM
			#pragma target 3.0
			#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 

			struct Input
			{
				float2 uv_texcoord;
			};

			uniform sampler2D _Normal;
			uniform float4 _Normal_ST;
			uniform sampler2D _Albedo;
			uniform float4 _Albedo_ST;
			float _Metallic;
			float _Glossiness;

			void surf(Input i , inout SurfaceOutputStandard o)
			{
				float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
				o.Normal = UnpackNormal(tex2D(_Normal, uv_Normal));
				float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
				o.Albedo = tex2D(_Albedo, uv_Albedo).rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = tex2D(_Albedo, uv_Albedo).a;
			}

			ENDCG


		}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
0;0;1920;1019;2073.339;767.0325;1.782607;True;False
Node;AmplifyShaderEditor.SamplerNode;5;-939.7501,-153.724;Inherit;True;Property;_Normal;Normal;3;0;Create;True;0;0;False;0;-1;None;0bb99517003a30e4fbc1a7df29ac13f2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-938.2844,-335.8495;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;-1;None;68ab35b818e08c44393121bfa921707b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-935.3748,235.0631;Inherit;True;Property;_AO;AO;0;0;Create;True;0;0;False;0;-1;None;23f224dc56f329d41bca59b326da87ab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-935.2769,39.0027;Inherit;True;Property;_MetallicSmooth;MetallicSmooth;2;0;Create;True;0;0;False;0;-1;None;ad91622bab1f80a4d8cc20e38a2b0b5f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;54,-174;Float;False;True;2;ASEMaterialInspector;0;0;Standard;xray;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;2;0
WireConnection;0;1;5;0
WireConnection;0;3;3;1
WireConnection;0;4;3;4
WireConnection;0;5;6;0
ASEEND*/
//CHKSM=46AC0153478C9A1912D83C76635A3E3CC7427431