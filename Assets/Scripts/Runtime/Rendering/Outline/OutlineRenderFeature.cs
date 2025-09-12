using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runtime.Rendering.Outline
{
    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        public Material blitMaterial;
        
        private OutlineRenderPass pass;
        
        public override void Create()
        {
            pass = new OutlineRenderPass(blitMaterial);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }
    }
}