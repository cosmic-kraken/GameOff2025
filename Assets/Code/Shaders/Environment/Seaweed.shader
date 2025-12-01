Shader "Custom/URP/SeaweedSway"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.1, 0.7, 0.3, 1)
        _SwayAmplitude("Sway Amplitude", Range(0, 1)) = 0.2
        _SwayFrequency("Sway Frequency", Range(0, 5)) = 1
        _SwaySpeed("Sway Speed", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _BaseColor;
            float _SwayAmplitude;
            float _SwayFrequency;
            float _SwaySpeed;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 pos = IN.positionOS.xyz;

                // Assume seaweed grows along +Y and pivot is at the base (y ≈ 0)
                float height = pos.z;

                // Top moves more than bottom (0–1 factor)
                float heightFactor = saturate(height);

                // Time in seconds
                float t = _Time.y * _SwaySpeed;

                // Phase varies by height, so it ripples up the stalk
                float sway = sin(t + height * _SwayFrequency) * _SwayAmplitude * heightFactor;

                // Apply sideways sway along X (use Z instead if your model is oriented differently)
                pos.x += sway;

                float4 posOS = float4(pos, 1.0);
                OUT.positionHCS = TransformObjectToHClip(posOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _BaseColor;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
