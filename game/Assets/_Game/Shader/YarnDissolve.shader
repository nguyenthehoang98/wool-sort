// YarnDissolve: shader len viết tay (không Amplify).
// - Màu len: lerp(_DarkThreadColor, _MainColor) theo pattern _Texture.
// - Tan dần (dissolve): wipe theo TRỤC CAO NHẤT của mesh (object space),
//   runtime (YarnPart) đo bounds từng mesh rồi đẩy _WipeAxis/_WipeMin/_WipeMax
//   qua MaterialPropertyBlock nên mọi model đều wipe hết đúng thời gian.
// - 2 núm điều khiển lúc chạy: _Dissolve 1->0, _Cutoff (Mask Clip) 0->1.
// - Tên property giữ giống ThreadMat_* cũ để migrate material không mất setting.
Shader "WoolSort/YarnDissolve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainColor ("Main Color", Color) = (0.5,0.5,0.5,1)
        _DarkThreadColor ("Dark Thread Color", Color) = (0.2,0.2,0.2,1)
        _Texture ("Thread Pattern", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 0.5
        _ThreadAnim ("Thread Anim", Float) = 0
        _ScrollAlongV ("Scroll Along V", Float) = 0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Float) = 1
        _AO ("AO", 2D) = "white" {}
        _Metallic ("Metallic", Float) = 0
        _Glossiness ("Smoothness", Float) = 0.34
        _DissolveNoise ("Dissolve Noise", 2D) = "white" {}
        _Dissolve ("Dissolve", Range(0, 1)) = 1
        _Cutoff ("Mask Clip Value", Float) = 0
        _WipeAxis ("Wipe Axis: 0=X, 1=Y, 2=Z (object space)", Float) = 1
        _WipeMin ("Wipe Min (object space)", Float) = 0
        _WipeMax ("Wipe Max (object space)", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _Texture;
        sampler2D _NormalMap;
        sampler2D _AO;
        sampler2D _DissolveNoise;
        fixed4 _Color;
        fixed4 _MainColor;
        fixed4 _DarkThreadColor;
        float _Tiling;
        float _ThreadAnim;
        float _ScrollAlongV;
        float _BumpScale;
        float _Metallic;
        float _Glossiness;
        float _Dissolve;
        float _Cutoff;
        float _WipeAxis;
        float _WipeMin;
        float _WipeMax;

        struct Input
        {
            float2 uv_Texture;
            float2 uv_NormalMap;
            float2 uv_AO;
            float2 uv_DissolveNoise;
            float3 objPos;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objPos = v.vertex.xyz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Màu sợi: pattern lerp tối<->sáng, scroll dọc khi _ThreadAnim bật
            float2 tuv = IN.uv_Texture * _Tiling;
            tuv.y += _Time.y * _ScrollAlongV * _ThreadAnim;
            fixed4 pattern = tex2D(_Texture, tuv);
            fixed3 albedo = lerp(_DarkThreadColor.rgb, _MainColor.rgb, pattern.r) * _Color.rgb;
            albedo *= tex2D(_AO, IN.uv_AO).r;
            o.Albedo = albedo;

            fixed3 n = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            n.xy *= _BumpScale;
            o.Normal = normalize(n);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;

            // Wipe theo trục cao nhất: tnorm 0 (đáy) -> 1 (đỉnh).
            float coord = _WipeAxis < 0.5 ? IN.objPos.x
                : (_WipeAxis < 1.5 ? IN.objPos.y : IN.objPos.z);
            float span = max(_WipeMax - _WipeMin, 1e-4);
            float tnorm = saturate((coord - _WipeMin) / span);
            // edge: 1 = đầy (giữ hết), 0 = hết (cắt hết).
            float edge = saturate(_Dissolve);
            float nz = tex2D(_DissolveNoise, IN.uv_DissolveNoise).r;
            // Mép wipe mềm + rỗ noise, rồi trừ ngưỡng cutoff toàn cục.
            float band = saturate((edge - tnorm) * 6.0 + nz);
            clip(band - _Cutoff - 1e-4);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
