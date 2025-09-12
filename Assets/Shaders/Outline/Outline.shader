Shader "Custom/Outline"
{
    Properties
    {
        _DepthThreshold ("Depth Threshold", float) = 1.5
        _NormalThreshold ("Normal Threshold", Range(0, 1)) = 0.1
        _DepthNormalThreshold ("Depth Normal Threshold", Range(0, 1)) = 0.5
        _DepthNormalThresholdScale ("Depth Normal Threshold Scale", float) = 7
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewSpaceDir : VIEW_DIR;
            };

            static const float2 verts[] = 
            {
                float2(-1, -1),
                float2(-1, 3),
                float2(3, -1),
            };
            
            Varyings vert (uint id : SV_VertexID)
            {
                Varyings output;
                output.vertex = float4(verts[id], 0, 1);
                output.uv = output.vertex * 0.5 + 0.5;
                output.uv.y = 1 - output.uv.y;
                output.viewSpaceDir = mul(unity_MatrixInvVP, output.vertex).xyz;
                return output;
            }

            static float4 _Scale = 1;
            
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;
            
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D(_CameraNormalsTexture);
            SAMPLER(sampler_CameraNormalsTexture);

            float _DepthThreshold;
            float _DepthNormalThreshold;
            float _DepthNormalThresholdScale;
            
            float _NormalThreshold;

            half4 frag (Varyings input) : SV_Target
            {
                float halfScaleFloor = floor(_Scale * 0.5);
                float halfScaleCeil = ceil(_Scale * 0.5);
                
                half2 uvs[] =
                {
                    input.uv - float2(_CameraOpaqueTexture_TexelSize.x, _CameraOpaqueTexture_TexelSize.y) * halfScaleFloor,
                    input.uv + float2(_CameraOpaqueTexture_TexelSize.x, _CameraOpaqueTexture_TexelSize.y) * halfScaleCeil,
                    input.uv + float2(_CameraOpaqueTexture_TexelSize.x * halfScaleCeil, -_CameraOpaqueTexture_TexelSize.y * halfScaleFloor),
                    input.uv + float2(-_CameraOpaqueTexture_TexelSize.x * halfScaleFloor, _CameraOpaqueTexture_TexelSize.y * halfScaleCeil),
                };
                
                half depth[] =
                {
                    SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uvs[0]).r,                    
                    SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uvs[1]).r,                    
                    SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uvs[2]).r,                    
                    SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uvs[3]).r,                    
                };

                half3 normals[] =
                {
                    SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uvs[0]).rgb,
                    SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uvs[1]).rgb,
                    SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uvs[2]).rgb,
                    SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uvs[3]).rgb,
                };

                float3 viewNormal = normals[0];
                float ndv = 1 - dot(viewNormal, -normalize(input.viewSpaceDir));
                float normalThreshold01 = saturate((ndv - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
                float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;
                float depthThreshold = _DepthThreshold * depth[0] * normalThreshold;
                
                half depthDiff0 = depth[1] - depth[0];
                half depthDiff1 = depth[3] - depth[2];
                half edgeDepth = sqrt(pow(depthDiff0, 2) + pow(depthDiff1, 2)) * 100;
                edgeDepth = edgeDepth > depthThreshold;

                half3 normalDiff0 = normals[1] - normals[0];
                half3 normalDiff1 = normals[3] - normals[2];
                half edgeNormal = sqrt(dot(normalDiff0, normalDiff0) + dot(normalDiff1, normalDiff1));
                edgeNormal = edgeNormal > _NormalThreshold;

                if (1-max(edgeDepth, edgeNormal)) discard;
                
                return float4(0, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
