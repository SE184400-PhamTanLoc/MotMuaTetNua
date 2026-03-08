//
// Outline Fill - URP
// Vẽ viền (expand mesh theo normal trong view space) chỉ ở vùng Stencil != 1.
//
Shader "Custom/Outline Fill URP"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+110"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Fill"
            Cull Off
            ZTest [_ZTest]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 smoothNormal : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 normalOS = any(input.smoothNormal.xyz) ? input.smoothNormal.xyz : input.normalOS;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                float3 viewPos = TransformWorldToView(positionWS);
                float3 viewNormal = normalize(TransformWorldToViewDir(normalWS));
                float scale = -viewPos.z * _OutlineWidth * 0.001;
                float3 viewOffset = viewNormal * scale;
                float3 newViewPos = viewPos + viewOffset;
                output.positionCS = TransformWViewToHClip(newViewPos);
                output.color = _OutlineColor;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}
