using UnityEngine;

namespace Runtime.Items
{
    public class GunEffects : MonoBehaviour, IGunEventListener
    {
        public ParticleSystem muzzleFlash;
        public ParticleSystem hitEffect;

        private void Awake()
        {
            var main = (ParticleSystem.MainModule)default;
            if (muzzleFlash != null)
            {
                main = muzzleFlash.main;
                main.loop = false;
                main.playOnAwake = false;
            }

            if (hitEffect != null)
            {
                main = hitEffect.main;
                main.loop = false;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }

        public void OnShoot()
        {
            if (!isActiveAndEnabled) return;

            if (muzzleFlash != null) muzzleFlash.Play(true);
        }

        public void OnReloadStart() { }

        public void OnReloadEnd() { }

        public void OnHit(RaycastHit hit)
        {
            if (!isActiveAndEnabled) return;

            if (hitEffect != null)
            {
                hitEffect.transform.position = hit.point;
                hitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);

                hitEffect.Play(true);
            }
        }
    }
}