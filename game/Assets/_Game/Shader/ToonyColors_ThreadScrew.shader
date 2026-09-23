// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonyColor_ThreadScrew"
{
	Properties
	{
		[Header(BASE COLOR)]_Texture("Texture", 2D) = "white" {}
		[Header(ROUGHNESS)]_RoughnessMap("RoughnessMap", 2D) = "white" {}
		[Header(AMBIENT OCCLUSION)]_AO("AO", 2D) = "white" {}
		[Header(NORMAL)]_NormalMap("Normal Map", 2D) = "bump" {}
		_VertexOffset("Vertex Offset", Range( -0.5 , 0)) = 0
		_Tiling("Tiling", Float) = 0
		_MainColor("Main Color", Color) = (0,0,0,0)
		_DarkThreadColor("DarkThread Color", Color) = (0,0,0,0)
		_AOcolor("AO color", Color) = (0,0,0,0)
		_Metallic("Metallic", Float) = 0
		_RoughnessMultiplier("Roughness Multiplier", Float) = 0
		[Header(SHADOW)]_ShadowColor("Shadow Color", Color) = (1,0.8915094,0.8915094,0)
		_MinShadow("Min Shadow", Float) = 0.14
		_MaxShadow("Max Shadow", Float) = 0.61
		[Header(HIGHLIGHT)]_Smoothness("Highlight Smoothness", Range( 0 , 1)) = 0.5516343
		[HDR]_HighlightColor("Highlight Color", Color) = (1,1,1,0)
		_PaintLayer("Paint Layer", Color) = (0,0,0,0)
		[Header(ANIMATION)]_ScrollAlongV("ScrollAlongV", Range( 0 , 5)) = 0
		_AOSHADOW("AO SHADOW", Range( 0 , 1)) = 0
		[Toggle]_ThreadAnim("Thread Anim", Float) = 0
		[Header(DISSOLVE)]_DissolveNoise("Dissolve Noise", 2D) = "white" {}
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv2_texcoord2;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
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

		uniform float _VertexOffset;
		uniform float _Dissolve;
		uniform sampler2D _DissolveNoise;
		uniform float4 _DissolveNoise_ST;
		uniform sampler2D _NormalMap;
		uniform float _ThreadAnim;
		uniform float _Tiling;
		uniform float _ScrollAlongV;
		uniform float4 _PaintLayer;
		uniform float4 _DarkThreadColor;
		uniform float4 _MainColor;
		uniform sampler2D _Texture;
		uniform float4 _AOcolor;
		uniform sampler2D _AO;
		uniform float _MinShadow;
		uniform float _MaxShadow;
		uniform float _AOSHADOW;
		uniform float4 _ShadowColor;
		uniform float _Smoothness;
		uniform float4 _HighlightColor;
		uniform sampler2D _RoughnessMap;
		uniform float _Metallic;
		uniform float _RoughnessMultiplier;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( _VertexOffset * ase_vertexNormal );
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float smoothstepResult602 = smoothstep( (0.0 + (_Dissolve - 0.0) * (2.0 - 0.0) / (1.0 - 0.0)) , 0.0 , i.uv2_texcoord2.y);
			float2 uv_DissolveNoise = i.uv_texcoord * _DissolveNoise_ST.xy + _DissolveNoise_ST.zw;
			SurfaceOutputStandard s562 = (SurfaceOutputStandard ) 0;
			s562.Albedo = float3( 0,0,0 );
			float2 appendResult662 = (float2(_Tiling , _Tiling));
			float2 uv_TexCoord658 = i.uv_texcoord * appendResult662;
			float2 appendResult656 = (float2(0 , ( _Time.y * _ScrollAlongV )));
			float2 uv_TexCoord646 = i.uv_texcoord + appendResult656;
			float3 NormalMap541 = UnpackNormal( tex2D( _NormalMap, (( _ThreadAnim )?( uv_TexCoord646 ):( uv_TexCoord658 )) ) );
			s562.Normal = WorldNormalVector( i , NormalMap541 );
			float4 lerpResult619 = lerp( _DarkThreadColor , _MainColor , tex2D( _Texture, (( _ThreadAnim )?( uv_TexCoord646 ):( uv_TexCoord658 )) ));
			float4 tex2DNode579 = tex2D( _AO, (( _ThreadAnim )?( uv_TexCoord646 ):( uv_TexCoord658 )) );
			float3 temp_output_21_0_g267 = NormalMap541;
			float3 newWorldNormal27_g267 = normalize( (WorldNormalVector( i , temp_output_21_0_g267 )) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult9_g267 = dot( newWorldNormal27_g267 , ase_worldlightDir );
			float temp_output_11_0_g267 = saturate( dotResult9_g267 );
			float smoothstepResult15_g267 = smoothstep( _MinShadow , _MaxShadow , temp_output_11_0_g267);
			float temp_output_3_0_g267 = ( 1.0 - smoothstepResult15_g267 );
			float temp_output_677_29 = temp_output_3_0_g267;
			float4 lerpResult624 = lerp( lerpResult619 , _AOcolor , ( ( 1.0 - tex2DNode579.r ) * (_AOSHADOW + (temp_output_677_29 - 0.0) * (1.0 - _AOSHADOW) / (1.0 - 0.0)) ));
			float4 temp_output_4_0_g267 = ( temp_output_3_0_g267 * _ShadowColor );
			float4 blendOpSrc638 = lerpResult624;
			float4 blendOpDest638 = temp_output_4_0_g267;
			float4 lerpBlendMode638 = lerp(blendOpDest638,2.0f*blendOpDest638*blendOpSrc638 + blendOpDest638*blendOpDest638*(1.0f - 2.0f*blendOpSrc638),temp_output_677_29);
			float4 lerpResult639 = lerp( lerpResult624 , ( saturate( lerpBlendMode638 )) , temp_output_677_29);
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult4_g268 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float dotResult6_g268 = dot( normalizeResult4_g268 , normalize( (WorldNormalVector( i , NormalMap541 )) ) );
			float temp_output_3_0_g269 = ( pow( dotResult6_g268 , exp2( ( ( _Smoothness * 10.0 ) + 1.0 ) ) ) - 0.1 );
			float4 Roughness566 = ( 1.0 - tex2D( _RoughnessMap, (( _ThreadAnim )?( uv_TexCoord646 ):( uv_TexCoord658 )) ) );
			s562.Emission = ( ( _PaintLayer * lerpResult639 ) + saturate( ( ( saturate( ( temp_output_3_0_g269 / fwidth( temp_output_3_0_g269 ) ) ) * _HighlightColor * _Smoothness ) * Roughness566.r ) ) ).rgb;
			s562.Metallic = _Metallic;
			s562.Smoothness = ( _RoughnessMultiplier * Roughness566 ).r;
			s562.Occlusion = tex2DNode579.r;

			data.light = gi.light;

			UnityGI gi562 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g562 = UnityGlossyEnvironmentSetup( s562.Smoothness, data.worldViewDir, s562.Normal, float3(0,0,0));
			gi562 = UnityGlobalIllumination( data, s562.Occlusion, s562.Normal, g562 );
			#endif

			float3 surfResult562 = LightingStandard ( s562, viewDir, gi562 ).rgb;
			surfResult562 += s562.Emission;

			#ifdef UNITY_PASS_FORWARDADD//562
			surfResult562 -= s562.Emission;
			#endif//562
			c.rgb = surfResult562;
			c.a = 1;
			clip( saturate( ( smoothstepResult602 / tex2D( _DissolveNoise, uv_DissolveNoise ) ) ).r - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv2_texcoord2;
				o.customPack1.xy = v.texcoord1;
				o.customPack1.zw = customInputData.uv_texcoord;
				o.customPack1.zw = v.texcoord;
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
				surfIN.uv2_texcoord2 = IN.customPack1.xy;
				surfIN.uv_texcoord = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
2117;140;1920;1011;353.8318;-586.1507;1.3;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;647;-1109.305,959.1635;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;650;-1176.305,1083.163;Inherit;False;Property;_ScrollAlongV;ScrollAlongV;19;1;[Header];Create;True;1;ANIMATION;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;652;-897.3049,1018.163;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;654;-907.3049,795.1635;Inherit;False;Constant;_Vector0;Vector 0;15;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;660;-838.835,664.2956;Inherit;False;Property;_Tiling;Tiling;5;0;Create;True;0;0;0;False;0;False;0;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;656;-698.3049,859.1635;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;662;-674.835,647.2956;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;658;-519.835,688.2956;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;646;-451.5416,914.4448;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;657;-239.835,782.3445;Inherit;False;Property;_ThreadAnim;Thread Anim;21;0;Create;True;0;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;530;113.9137,1630.965;Inherit;True;Property;_NormalMap;Normal Map;3;1;[Header];Create;True;1;NORMAL;0;0;False;0;False;-1;None;5f6303520beb3eb4e9258c5d1a25f1ba;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;541;425.9767,1682.252;Inherit;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;542;444.8144,1543.175;Inherit;False;541;NormalMap;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;677;681.1564,1534.606;Inherit;False;ToonShadow 1;11;;267;ff8018553db5933488f2570ece675d40;0;1;21;FLOAT3;0,0,0;False;2;FLOAT;29;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;683;662.7681,1376.551;Inherit;False;Property;_AOSHADOW;AO SHADOW;20;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;579;36.05613,1287.399;Inherit;True;Property;_AO;AO;2;1;[Header];Create;True;1;AMBIENT OCCLUSION;0;0;False;0;False;-1;None;04fd53a0182758a43be41db723994a39;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;623;341.7345,1285.611;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;248;129.3384,1087.39;Inherit;False;Property;_MainColor;Main Color;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.3066038,0.3066038,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;234;129.2513,603.2574;Inherit;True;Property;_Texture;Texture;0;1;[Header];Create;True;1;BASE COLOR;0;0;False;0;False;-1;None;b6bf9e06c4594334892b03d54b31e1ee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;554;139.3858,903.562;Inherit;False;Property;_DarkThreadColor;DarkThread Color;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.9333334,0.2666667,0.2666667,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;682;882.4681,1353.151;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.4;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;678;855.3714,1228.49;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;564;2670.249,1137.978;Inherit;True;Property;_RoughnessMap;RoughnessMap;1;1;[Header];Create;True;1;ROUGHNESS;0;0;False;0;False;-1;None;b7f6cc0cd88a34c4a83f6de83c34f78e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;619;817.2438,851.7205;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;625;427.3352,1027.038;Inherit;False;Property;_AOcolor;AO color;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.764151,0.05406725,0.1816966,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;594;3019.431,1135.789;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;624;1013.177,991.3478;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;566;3221.546,1112.453;Inherit;False;Roughness;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;609;2627.878,487.6967;Inherit;False;Property;_Dissolve;Dissolve;23;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;638;1394.447,1230.102;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;524;2762.778,290.4966;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;611;2968.259,495.8235;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;543;1431.707,1521.193;Inherit;False;541;NormalMap;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;568;1425.785,1404.776;Inherit;False;566;Roughness;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;645;1540.22,664.6868;Inherit;False;Property;_PaintLayer;Paint Layer;18;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;639;1588.949,926.5825;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;599;3129.37,989.9218;Inherit;False;Property;_RoughnessMultiplier;Roughness Multiplier;10;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;644;1979.249,860.1025;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;602;3115.339,154.7563;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.06;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;601;1676.693,1434.131;Inherit;False;ToonHighlight;15;;268;c04d4e6e9d2d1bc4fa3c199896f62ed7;0;2;21;FLOAT;0;False;17;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;610;3164.452,476.0098;Inherit;True;Property;_DissolveNoise;Dissolve Noise;22;1;[Header];Create;True;1;DISSOLVE;0;0;False;0;False;-1;None;b7f6cc0cd88a34c4a83f6de83c34f78e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;607;3515.679,246.2126;Inherit;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;598;3395.083,1007.659;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;672;3761.443,746.075;Inherit;False;Property;_VertexOffset;Vertex Offset;4;0;Create;True;0;0;0;False;0;False;0;0;-0.5;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;640;3390.357,917.4442;Inherit;False;Property;_Metallic;Metallic;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;2289.722,866.4014;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;675;3837.864,950.7761;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;593;3247.276,813.3427;Inherit;False;541;NormalMap;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;674;4232.443,815.075;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;608;3765.604,245.9431;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomStandardSurface;562;3566.989,802.2103;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;676;4046.611,743.5996;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;186;4292.027,362.8358;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;ToonyColor_ThreadScrew;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;24;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;652;0;647;0
WireConnection;652;1;650;0
WireConnection;656;0;654;1
WireConnection;656;1;652;0
WireConnection;662;0;660;0
WireConnection;662;1;660;0
WireConnection;658;0;662;0
WireConnection;646;1;656;0
WireConnection;657;0;658;0
WireConnection;657;1;646;0
WireConnection;530;1;657;0
WireConnection;541;0;530;0
WireConnection;677;21;542;0
WireConnection;579;1;657;0
WireConnection;623;0;579;1
WireConnection;234;1;657;0
WireConnection;682;0;677;29
WireConnection;682;3;683;0
WireConnection;678;0;623;0
WireConnection;678;1;682;0
WireConnection;564;1;657;0
WireConnection;619;0;554;0
WireConnection;619;1;248;0
WireConnection;619;2;234;0
WireConnection;594;0;564;0
WireConnection;624;0;619;0
WireConnection;624;1;625;0
WireConnection;624;2;678;0
WireConnection;566;0;594;0
WireConnection;638;0;624;0
WireConnection;638;1;677;0
WireConnection;638;2;677;29
WireConnection;611;0;609;0
WireConnection;639;0;624;0
WireConnection;639;1;638;0
WireConnection;639;2;677;29
WireConnection;644;0;645;0
WireConnection;644;1;639;0
WireConnection;602;0;524;2
WireConnection;602;1;611;0
WireConnection;601;21;568;0
WireConnection;601;17;543;0
WireConnection;607;0;602;0
WireConnection;607;1;610;0
WireConnection;598;0;599;0
WireConnection;598;1;566;0
WireConnection;121;0;644;0
WireConnection;121;1;601;0
WireConnection;674;0;672;0
WireConnection;674;1;675;0
WireConnection;608;0;607;0
WireConnection;562;1;593;0
WireConnection;562;2;121;0
WireConnection;562;3;640;0
WireConnection;562;4;598;0
WireConnection;562;5;579;1
WireConnection;676;0;672;0
WireConnection;186;10;608;0
WireConnection;186;13;562;0
WireConnection;186;11;674;0
ASEEND*/
//CHKSM=6705439062CBBD5C88FE40D718268117AA7DE595