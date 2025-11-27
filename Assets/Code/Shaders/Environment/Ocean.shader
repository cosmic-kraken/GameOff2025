Shader "Ocean/Ocean_URP_LitTransparent"
{
    Properties
    {
        _SeaColor("Sea Color", Color) = (0, 0.2, 0.5, 1)
        _SkyColor("Sky Color", Color) = (0.4, 0.6, 1, 1)
        _FresnelLookUp("Fresnel Lookup", 2D) = "white" {}

        _ShallowAlpha("Alpha In Strong Light", Range(0,1)) = 0.2 // more transparent
        _DeepAlpha("Alpha In Weak Light", Range(0,1)) = 0.9      // more opaque
        _Ambient("Ambient Lighting", Range(0,2)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent-100"              // EARLIER than normal Transparent
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off              // see from above & below
            ZWrite Off            // transparency
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // Main light & shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _SeaColor;
            float4 _SkyColor;

            float  _ShallowAlpha;
            float  _DeepAlpha;
            float  _Ambient;

            TEXTURE2D(_FresnelLookUp);
            SAMPLER(sampler_FresnelLookUp);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.normalWS    = normalize(normalWS);
                OUT.viewDirWS   = normalize(_WorldSpaceCameraPos.xyz - positionWS);
                OUT.positionWS  = positionWS;

                return OUT;
            }

            float4 frag(Varyings IN, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                float3 geomNormalWS    = normalize(IN.normalWS);
                float3 viewDirectionWS = normalize(IN.viewDirWS);

                // For Fresnel we flip on backfaces so it stays smooth across the surface
                float3 fresnelNormalWS = frontFace ? geomNormalWS : -geomNormalWS;

                // -------- Fresnel color (sea ↔ sky) --------
                float cosTheta = abs(dot(viewDirectionWS, fresnelNormalWS));
                float2 fresnelUV = float2(cosTheta, 0.0);
                float4 fresnelSample = SAMPLE_TEXTURE2D(_FresnelLookUp, sampler_FresnelLookUp, fresnelUV);
                float  fresnelFactor = fresnelSample.a;

                float3 baseSeaColor = lerp(_SeaColor.rgb, _SkyColor.rgb, fresnelFactor);

                // -------- Main light (sun) --------
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                // abs so both sides react to light (for underwater)
                float ndotl = abs(dot(geomNormalWS, mainLight.direction));
                float3 diffuseLighting = ndotl * mainLight.color * mainLight.shadowAttenuation;

                float3 ambientLighting = _Ambient.xxx;
                float3 totalLighting = ambientLighting + diffuseLighting;

                float3 finalColor = baseSeaColor * totalLighting;

                // -------- Alpha from light intensity --------
                float lightIntensity = saturate(ndotl * mainLight.shadowAttenuation);

                // lightIntensity = 0 -> _DeepAlpha (dark, opaque)
                // lightIntensity = 1 -> _ShallowAlpha (bright, transparent)
                float alpha = lerp(_DeepAlpha, _ShallowAlpha, lightIntensity);

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
