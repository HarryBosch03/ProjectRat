#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "OkLab.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float4 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float4 normalWS : NORMAL;
    float2 uv     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    float3 normalWS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct Surface
{
    float3 viewDirection;
};

TEXTURE2D(_HighColorMap);
SAMPLER(sampler_HighColorMap);

TEXTURE2D(_LowColorMap);
SAMPLER(sampler_LowColorMap);

TEXTURE2D(_AttenuationMap);
SAMPLER(sampler_AttenuationMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseColorHigh;
float4 _BaseColorLow;
CBUFFER_END

Varyings ToonPassVertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    output.uv = input.uv;
    
    return output;
}

half3 OkLabLerp(half3 a, half3 b, half t)
{
    return oklabToRgb(lerp(rgbToOklab(a), rgbToOklab(b), t));
}

half3 CalculateLighting(Light light, Varyings input, Surface surface, half3 albedoHigh, half3 albedoLow)
{
    float ndl = max(0, dot(light.direction, input.normalWS));
    float attenuation = ndl * light.distanceAttenuation * light.shadowAttenuation;

    float ndh = 0;
    //float h = normalize(light.direction + surface.viewDirection);
    //ndh = saturate(dot(h, input.normalWS));
    //ndh = pow(ndh, 128);

    attenuation = SAMPLE_TEXTURE2D(_AttenuationMap, sampler_AttenuationMap, attenuation + ndh) * 2;

    return OkLabLerp(albedoLow, albedoHigh, attenuation) * light.color;
}

half4 ToonPassFragment(Varyings input) : SV_TARGET
{
    input.normalWS = normalize(input.normalWS);
    
    half3 albedoHigh = _BaseColorHigh.rgb * SAMPLE_TEXTURE2D(_HighColorMap, sampler_HighColorMap, input.uv);
    half3 albedoLow = _BaseColorLow.rgb * SAMPLE_TEXTURE2D(_LowColorMap, sampler_LowColorMap, input.uv);
    half alpha = _BaseColorHigh.a;

    half4 shadowCoords = TransformWorldToShadowCoord(input.positionWS);

    Surface surface;
    surface.viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
    
    half3 color = 0;
    color += CalculateLighting(GetMainLight(shadowCoords), input, surface, albedoHigh, albedoLow);

    return half4(color, alpha);
}