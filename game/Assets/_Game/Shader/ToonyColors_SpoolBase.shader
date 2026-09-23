// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonyColor_SpoolBase"
{
	Properties
	{
		_MainColor("Main Color", Color) = (0,0,0,0)
		[Header(SHADOW)]_ShadowColor("Shadow Color", Color) = (1,0.8915094,0.8915094,0)
		_MinShadow("Min Shadow", Float) = 0.14
		_MaxShadow("Max Shadow", Float) = 0.61
		_PaintLayer("Paint Layer", Color) = (0,0,0,0)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[Toggle(_USINGGRADIENT_ON)] _USINGGRADIENT("USING GRADIENT", Float) = 0
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
		#pragma shader_feature_local _USINGGRADIENT_ON
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			float3 worldPos;
			float4 vertexColor : COLOR;
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
		uniform float4 _MainColor;
		uniform sampler2D _TextureSample0;
		uniform float _MinShadow;
		uniform float _MaxShadow;
		uniform float4 _ShadowColor;
		uniform float4 _PaintLayer;
		uniform float _HighlightMin;
		uniform float _HighlightMax;
		uniform float4 _HighlightColor;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef _USINGGRADIENT_ON
				float4 staticSwitch706 = tex2D( _TextureSample0, i.uv_texcoord );
			#else
				float4 staticSwitch706 = _MainColor;
			#endif
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult9_g7 = dot( ase_normWorldNormal , ase_worldlightDir );
			float temp_output_11_0_g7 = saturate( dotResult9_g7 );
			float smoothstepResult15_g7 = smoothstep( _MinShadow , _MaxShadow , temp_output_11_0_g7);
			float temp_output_3_0_g7 = ( 1.0 - smoothstepResult15_g7 );
			float4 temp_output_4_0_g7 = ( temp_output_3_0_g7 * _ShadowColor );
			float4 blendOpSrc638 = _MainColor;
			float4 blendOpDest638 = temp_output_4_0_g7;
			float temp_output_708_29 = temp_output_3_0_g7;
			float4 lerpBlendMode638 = lerp(blendOpDest638,2.0f*blendOpDest638*blendOpSrc638 + blendOpDest638*blendOpDest638*(1.0f - 2.0f*blendOpSrc638),temp_output_708_29);
			float4 lerpResult639 = lerp( staticSwitch706 , ( saturate( lerpBlendMode638 )) , temp_output_708_29);
			float4 lerpResult644 = lerp( lerpResult639 , ( lerpResult639 * _PaintLayer ) , i.vertexColor.r);
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult4_g8 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float dotResult6_g8 = dot( normalizeResult4_g8 , ase_normWorldNormal );
			float smoothstepResult24_g8 = smoothstep( _HighlightMin , _HighlightMax , dotResult6_g8);
			c.rgb = ( lerpResult644 + saturate( ( smoothstepResult24_g8 * _HighlightColor ) ) ).rgb;
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
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 

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
				half4 color : COLOR0;
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
				o.color = v.color;
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
				surfIN.vertexColor = IN.color;
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
1920;0;1920;1011;-2353.189;-125.0388;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;704;-50.83163,389.461;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;248;129.3384,1087.39;Inherit;False;Property;_MainColor;Main Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.9528301,0.9528301,0.9528301,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;692;627.031,487.852;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;39d26e54884eed3448b487bf0ead7dea;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;708;839.2939,1309.92;Inherit;False;ToonShadow 1;1;;7;ff8018553db5933488f2570ece675d40;0;0;2;FLOAT;29;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;638;1293.88,1222.134;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;706;1032.861,823.078;Inherit;False;Property;_USINGGRADIENT;USING GRADIENT;7;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;639;1393.085,922.3246;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;646;1558.891,693.0828;Inherit;False;Property;_PaintLayer;Paint Layer;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.8980393,0.9019608,0.8705883,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;645;1699.003,1007.066;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;640;1691.135,1132.266;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;644;1961.357,801.6945;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;707;1676.693,1434.131;Inherit;False;ToonHighlight;9;;8;c04d4e6e9d2d1bc4fa3c199896f62ed7;0;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;2172.534,931.2039;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;709;3346.189,528.0388;Inherit;False;Property;_Opacity;Opacity;8;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;186;3931.255,426.2992;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;ToonyColor_SpoolBase;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;692;1;704;0
WireConnection;638;0;248;0
WireConnection;638;1;708;0
WireConnection;638;2;708;29
WireConnection;706;1;248;0
WireConnection;706;0;692;0
WireConnection;639;0;706;0
WireConnection;639;1;638;0
WireConnection;639;2;708;29
WireConnection;645;0;639;0
WireConnection;645;1;646;0
WireConnection;644;0;639;0
WireConnection;644;1;645;0
WireConnection;644;2;640;1
WireConnection;121;0;644;0
WireConnection;121;1;707;0
WireConnection;186;9;709;0
WireConnection;186;13;121;0
ASEEND*/
//CHKSM=83252912D783E3777CF4E9A7D47AB59458842424