// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "@Xxuebi/Shield"
{
	Properties
	{
		[HDR]_Main_Color("Main_Color", Color) = (0,0,0,0)
		_Main_Tex("Main_Tex", 2D) = "white" {}
		_Main_Int("Main_Int", Float) = 1
		_Main_Speed("Main_Speed", Vector) = (0,0,0,0)
		_Noise_Tex("Noise_Tex", 2D) = "white" {}
		_Noise_UV("Noise_UV", Vector) = (0,0,0,0)
		_Noise_Speed("Noise_Speed", Vector) = (0,0,0,0)
		_Noise_Int("Noise_Int", Float) = 0
		_Rim_Bias("Rim_Bias", Float) = 0
		_Rim_Scale("Rim_Scale", Float) = 0
		_Rim_Power("Rim_Power", Float) = 0
		_All_Int("All_Int", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float4 _Main_Color;
		uniform float _Noise_Int;
		uniform sampler2D _Noise_Tex;
		uniform float2 _Noise_Speed;
		uniform float2 _Noise_UV;
		uniform sampler2D _Main_Tex;
		uniform float2 _Main_Speed;
		uniform float4 _Main_Tex_ST;
		uniform float _Main_Int;
		uniform float _Rim_Bias;
		uniform float _Rim_Scale;
		uniform float _Rim_Power;
		uniform float _All_Int;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float2 uv_TexCoord19 = i.uv_texcoord * _Noise_UV;
			float2 panner24 = ( 1.0 * _Time.y * _Noise_Speed + uv_TexCoord19);
			float2 uv0_Main_Tex = i.uv_texcoord * _Main_Tex_ST.xy + _Main_Tex_ST.zw;
			float2 panner39 = ( 1.0 * _Time.y * _Main_Speed + uv0_Main_Tex);
			float4 temp_output_20_0 = ( ( _Noise_Int * tex2D( _Noise_Tex, panner24 ).r ) + ( tex2D( _Main_Tex, panner39 ) * _Main_Int ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV3 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode3 = ( _Rim_Bias + _Rim_Scale * pow( 1.0 - fresnelNdotV3, _Rim_Power ) );
			c.rgb = 0;
			c.a = ( i.vertexColor.a * ( ( temp_output_20_0 * saturate( fresnelNode3 ) ) + _All_Int ) ).r;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float2 uv_TexCoord19 = i.uv_texcoord * _Noise_UV;
			float2 panner24 = ( 1.0 * _Time.y * _Noise_Speed + uv_TexCoord19);
			float2 uv0_Main_Tex = i.uv_texcoord * _Main_Tex_ST.xy + _Main_Tex_ST.zw;
			float2 panner39 = ( 1.0 * _Time.y * _Main_Speed + uv0_Main_Tex);
			float4 temp_output_20_0 = ( ( _Noise_Int * tex2D( _Noise_Tex, panner24 ).r ) + ( tex2D( _Main_Tex, panner39 ) * _Main_Int ) );
			o.Emission = ( ( _Main_Color * temp_output_20_0 ) * i.vertexColor ).rgb;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17700
153;483;1252;673;1392.691;329.7762;2.30991;True;True
Node;AmplifyShaderEditor.Vector2Node;18;-1984.345,-795.5484;Inherit;False;Property;_Noise_UV;Noise_UV;5;0;Create;True;0;0;False;0;0,0;3,0.8;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1733.331,-265.0518;Inherit;False;0;12;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;38;-1716.399,-76.67801;Inherit;False;Property;_Main_Speed;Main_Speed;3;0;Create;True;0;0;False;0;0,0;0,0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;25;-1910.079,-526.1151;Inherit;False;Property;_Noise_Speed;Noise_Speed;6;0;Create;True;0;0;False;0;0,0;0,0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-1734.413,-678.0184;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;39;-1364.333,-165.3647;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;24;-1499.938,-597.7211;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-969.1565,-660.5502;Inherit;False;Property;_Noise_Int;Noise_Int;7;0;Create;True;0;0;False;0;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;32;-1165.984,-592.6698;Inherit;True;Property;_Noise_Tex;Noise_Tex;4;0;Create;True;0;0;False;0;-1;09f4a30c1a0141b5aaa317dfad6ebb90;8f354e6278ff9d5408958932b55aed52;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-568.1218,187.9141;Inherit;False;Property;_Rim_Bias;Rim_Bias;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-804.5447,-202.6164;Inherit;False;Property;_Main_Int;Main_Int;2;0;Create;True;0;0;False;0;1;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-1109.949,-253.9872;Inherit;True;Property;_Main_Tex;Main_Tex;1;0;Create;True;0;0;False;0;-1;None;8db0a1bd0e3949a4fb337557cfd7aabc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-569.4208,290.614;Inherit;False;Property;_Rim_Scale;Rim_Scale;9;0;Create;True;0;0;False;0;0;0.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-551.2208,402.4141;Inherit;False;Property;_Rim_Power;Rim_Power;10;0;Create;True;0;0;False;0;0;1.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-657.5022,-298.5583;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-767.3945,-497.0791;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;3;-355.9164,135.5865;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-500.2727,-359.3568;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;8;-33.13301,96.93863;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-135.0733,-742.2654;Inherit;False;Property;_Main_Color;Main_Color;0;1;[HDR];Create;True;0;0;False;0;0,0,0,0;1.517324,2.520532,4.486002,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;204.4937,-33.92953;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;34;354.4748,196.9708;Inherit;False;Property;_All_Int;All_Int;11;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;456.8805,194.0975;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;5;27.82748,-260.9691;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;68.11988,-531.0994;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;791.6812,-108.4803;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;301.5567,-292.2587;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;963.4822,-293.0523;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;@Xxuebi/Shield;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-0.18;1,1,1,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;18;0
WireConnection;39;0;13;0
WireConnection;39;2;38;0
WireConnection;24;0;19;0
WireConnection;24;2;25;0
WireConnection;32;1;24;0
WireConnection;12;1;39;0
WireConnection;28;0;12;0
WireConnection;28;1;29;0
WireConnection;26;0;27;0
WireConnection;26;1;32;1
WireConnection;3;1;9;0
WireConnection;3;2;10;0
WireConnection;3;3;11;0
WireConnection;20;0;26;0
WireConnection;20;1;28;0
WireConnection;8;0;3;0
WireConnection;6;0;20;0
WireConnection;6;1;8;0
WireConnection;33;0;6;0
WireConnection;33;1;34;0
WireConnection;4;0;2;0
WireConnection;4;1;20;0
WireConnection;37;0;5;4
WireConnection;37;1;33;0
WireConnection;7;0;4;0
WireConnection;7;1;5;0
WireConnection;0;2;7;0
WireConnection;0;9;37;0
ASEEND*/
//CHKSM=FB06D18FCF2C13EECC7F0F1E7850D69F1A7149A0