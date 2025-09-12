using UnityEngine.Rendering.Universal;

namespace Runtime.Rendering.DecalParticleSystem
{
    public class DecalParticleSystemRenderFeature : ScriptableRendererFeature
    {
        private DecalParticleSystemPass decalParticleSystemPass;
        
        public override void Create()
        {
            decalParticleSystemPass = new DecalParticleSystemPass();
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(decalParticleSystemPass);
        }
    }
}