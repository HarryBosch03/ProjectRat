using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Runtime.Rendering.DecalParticleSystem
{
    public class DecalParticleSystemPass : ScriptableRenderPass
    {
        public static List<DecalParticleSystem> renderers = new List<DecalParticleSystem>();
        
        public DecalParticleSystemPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var urpResources = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var colorTarget = urpResources.activeColorTexture;
            var mask = cameraData.camera.cullingMask;
            
            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if ((mask & (1 << renderer.gameObject.layer)) == 0) continue;
                
                using (var builder = renderGraph.AddRasterRenderPass($"[DecalParticleSystemPass].{renderer.name}", out PassData passData))
                {
                    builder.SetRenderAttachment(colorTarget, 0, AccessFlags.Write);

                    passData.mesh = renderer.quad;
                    passData.material = renderer.material;
                    passData.matrixList = renderer.evaluatedParticlePositions;

                    builder.SetRenderFunc<PassData>(Render);
                }
            }
        }

        private void Render(PassData data, RasterGraphContext ctx)
        {
            var cmd = ctx.cmd;
            cmd.DrawMeshInstanced(data.mesh, 0, data.material, 0, data.matrixList, data.matrixList.Length);
        }

        public class PassData
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4[] matrixList;
        }
    }
}