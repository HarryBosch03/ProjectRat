#include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

TEXTURE2D(_NoiseMap);
SAMPLER(sampler_NoiseMap);

TEXTURE2D(_AttenuationMap);
SAMPLER(sampler_AttenuationMap);

TEXTURE2D(_HighColorMap);
SAMPLER(sampler_HighColorMap);

TEXTURE2D(_LowColorMap);
SAMPLER(sampler_LowColorMap);

float4 _BaseColorHigh;

inline void InitializeStandardLitSurfaceDataToon(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColorHigh, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif
}

half3 RimLighting(InputData inputData, SurfaceData surfaceData, float3 bias)
{
    half3 color = 0;
    half3 ndv = 1 - dot(inputData.normalWS, inputData.viewDirectionWS);

    float3 biasWS = normalize(mul(unity_CameraToWorld, bias));
    color += (ndv > 0.75) * max(0, dot(biasWS, inputData.normalWS));
    
    return color;
}

half3 ToonLighting(InputData inputData, SurfaceData surfaceData, Light light, float specularMask)
{
    float ndl = max(0, dot(light.direction, inputData.normalWS));
    float attenuation = ndl * light.distanceAttenuation * light.shadowAttenuation;
    attenuation = SAMPLE_TEXTURE2D(_AttenuationMap, sampler_AttenuationMap, attenuation) * 2;
    
    half3 color = surfaceData.albedo * attenuation * light.color;

    float3 h = normalize(light.direction + inputData.viewDirectionWS);
    float ndh = max(0, dot(h, inputData.normalWS));

    if (ndh > lerp(1, 0.8, surfaceData.smoothness)) color += attenuation * specularMask;
    
    return color;
}

half4 ToonLighting(InputData input, SurfaceData surfaceData)
{
    half3 final = 0;

    half4 shadowMask = CalculateShadowMask(input);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(input, surfaceData);
    Light mainLight = GetMainLight(input, shadowMask, aoFactor);

    float2 screenUV = (input.normalizedScreenSpaceUV * _ScreenParams.xy) / min(_ScreenParams.x, _ScreenParams.y); 
    float noise = screenUV.x - screenUV.y;
    float specularMask = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, noise * 0.007);
    //specularMask = saturate(specularMask * 3 - 1);
    specularMask = specularMask > 0.5;

    final += surfaceData.albedo * 0.7;
    
    final += ToonLighting(input, surfaceData, mainLight, specularMask);

    return half4(final, surfaceData.alpha);
}

void ToonPassFragment(
    Varyings input
    , out half4 outColor : SV_Target0
    #ifdef _WRITE_RENDERING_LAYERS
, out float4 outRenderingLayers : SV_Target1
    #endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_PARALLAXMAP)
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
    #else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
    #endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
    #endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceDataToon(input.uv, surfaceData);

    #ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
    #endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));

    #if defined(_DBUFFER)
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif

    InitializeBakedGIData(input, inputData);

    half4 color = ToonLighting(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    outColor = color;

    #ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}
