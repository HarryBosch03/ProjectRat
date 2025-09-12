using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Runtime.Rendering.Outline
{
    public class OutlineRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial;

        public OutlineRenderPass(Material blitMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            this.blitMaterial = blitMaterial;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (blitMaterial == null) return;
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline", out var passData))
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                builder.UseTexture(resourceData.cameraDepthTexture);

                passData.colorTexture = resourceData.cameraColor;
                passData.depthTexture = resourceData.cameraDepthTexture;
                
                builder.AllowPassCulling(false);
                builder.SetRenderFunc<PassData>(Render);
            }
        }

        private void Render(PassData data, RasterGraphContext ctx)
        {
            var cmd = ctx.cmd;

            cmd.SetGlobalTexture("_SceneColor", data.colorTexture);
            cmd.SetGlobalTexture("_SceneDepth", data.depthTexture);
            
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);
        }

        private class PassData
        {
            public TextureHandle colorTexture;
            public TextureHandle depthTexture;
        }
    }
}