// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonyColor"
{
	Properties
	{
		[Header(BASE COLOR)]_Texture("Texture", 2D) = "white" {}
		_ChangeColor("Change Color", Color) = (0,0,0,0)
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		[Header(SHADOW)]_ShadowColor("Shadow Color", Color) = (1,0.8915094,0.8915094,0)
		_MinShadow("Min Shadow", Float) = 0.14
		_MaxShadow("Max Shadow", Float) = 0.61
		_ColorMask("Color Mask", 2D) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 0
		[HDR]_HighlightColor("Highlight Color", Color) = (1,1,1,0)
		_HighlightMax("HighlightMax", Float) = 0
		_HighlightMin("HighlightMin", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		struct Input
		{
			float3 worldNormal;
			float2 uv_texcoord;
			float3 worldPos;
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

		uniform float _Opacity;
		uniform sampler2D _Texture;
		uniform float4 _Texture_ST;
		uniform float4 _ColorTint;
		uniform float4 _ChangeColor;
		uniform sampler2D _ColorMask;
		uniform float4 _ColorMask_ST;
		uniform float _MinShadow;
		uniform float _MaxShadow;
		uniform float4 _ShadowColor;
		uniform float _HighlightMin;
		uniform float _HighlightMax;
		uniform float4 _HighlightColor;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s652 = (SurfaceOutputStandard ) 0;
			s652.Albedo = float3( 0,0,0 );
			float3 ase_worldNormal = i.worldNormal;
			s652.Normal = ase_worldNormal;
			float2 uv_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
			float2 uv_ColorMask = i.uv_texcoord * _ColorMask_ST.xy + _ColorMask_ST.zw;
			float4 lerpResult649 = lerp( ( tex2D( _Texture, uv_Texture ) * _ColorTint ) , _ChangeColor , tex2D( _ColorMask, uv_ColorMask ));
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult9_g3 = dot( ase_normWorldNormal , ase_worldlightDir );
			float temp_output_11_0_g3 = saturate( dotResult9_g3 );
			float smoothstepResult15_g3 = smoothstep( _MinShadow , _MaxShadow , temp_output_11_0_g3);
			float temp_output_3_0_g3 = ( 1.0 - smoothstepResult15_g3 );
			float4 temp_output_4_0_g3 = ( temp_output_3_0_g3 * _ShadowColor );
			float4 blendOpSrc638 = lerpResult649;
			float4 blendOpDest638 = temp_output_4_0_g3;
			float temp_output_643_29 = temp_output_3_0_g3;
			float4 lerpBlendMode638 = lerp(blendOpDest638,2.0f*blendOpDest638*blendOpSrc638 + blendOpDest638*blendOpDest638*(1.0f - 2.0f*blendOpSrc638),temp_output_643_29);
			float4 lerpResult639 = lerp( lerpResult649 , ( saturate( lerpBlendMode638 )) , temp_output_643_29);
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult4_g256 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float dotResult6_g256 = dot( normalizeResult4_g256 , ase_normWorldNormal );
			float smoothstepResult24_g256 = smoothstep( _HighlightMin , _HighlightMax , dotResult6_g256);
			s652.Emission = ( lerpResult639 + saturate( ( smoothstepResult24_g256 * _HighlightColor ) ) ).rgb;
			s652.Metallic = 0.0;
			s652.Smoothness = 0.0;
			s652.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi652 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g652 = UnityGlossyEnvironmentSetup( s652.Smoothness, data.worldViewDir, s652.Normal, float3(0,0,0));
			gi652 = UnityGlobalIllumination( data, s652.Occlusion, s652.Normal, g652 );
			#endif

			float3 surfResult652 = LightingStandard ( s652, viewDir, gi652 ).rgb;
			surfResult652 += s652.Emission;

			#ifdef UNITY_PASS_FORWARDADD//652
			surfResult652 -= s652.Emission;
			#endif//652
			c.rgb = surfResult652;
			c.a = _Opacity;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
1920;0;1920;1011;-1921.109;-370.8536;1.566354;True;False
Node;AmplifyShaderEditor.SamplerNode;234;116.5258,703.6472;Inherit;True;Property;_Texture;Texture;0;1;[Header];Create;True;1;BASE COLOR;0;0;False;0;False;-1;None;3fbcf9992604fa64a9872a3a1e02277f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;650;136.5099,1332.013;Inherit;False;Property;_ColorTint;Color Tint;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;648;99.86739,919.1755;Inherit;True;Property;_ColorMask;Color Mask;8;0;Create;True;0;0;0;False;0;False;-1;None;95aa77b8a6ff08143839410177203dd4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;646;463.459,785.4688;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;647;153.827,1130.491;Inherit;False;Property;_ChangeColor;Change Color;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;649;606.7534,920.3969;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;643;733.7564,1194.106;Inherit;False;ToonShadow 1;4;;3;ff8018553db5933488f2570ece675d40;0;0;2;FLOAT;29;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;638;1344.447,1171.102;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;639;1603.876,896.7299;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;601;1675.693,1434.131;Inherit;False;ToonHighlight;10;;256;c04d4e6e9d2d1bc4fa3c199896f62ed7;0;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;2289.722,866.4014;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomStandardSurface;652;2855.106,841.7681;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;651;3701.872,810.7548;Inherit;False;Property;_Opacity;Opacity;9;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;541;425.9767,1682.252;Inherit;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;530;113.9137,1630.965;Inherit;True;Property;_NormalMap;Normal Map;1;1;[Header];Create;True;1;NORMAL;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;542;536.2145,1407.575;Inherit;False;541;NormalMap;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;543;1431.707,1521.193;Inherit;False;541;NormalMap;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;186;4089.255,636.9301;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;ToonyColor;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;646;0;234;0
WireConnection;646;1;650;0
WireConnection;649;0;646;0
WireConnection;649;1;647;0
WireConnection;649;2;648;0
WireConnection;638;0;649;0
WireConnection;638;1;643;0
WireConnection;638;2;643;29
WireConnection;639;0;649;0
WireConnection;639;1;638;0
WireConnection;639;2;643;29
WireConnection;121;0;639;0
WireConnection;121;1;601;0
WireConnection;652;2;121;0
WireConnection;541;0;530;0
WireConnection;186;9;651;0
WireConnection;186;13;652;0
ASEEND*/
//CHKSM=BED0E3A5C932C90C283ED441FEDF3F5B1EB90412