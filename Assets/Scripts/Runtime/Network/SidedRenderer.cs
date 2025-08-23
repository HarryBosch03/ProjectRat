using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime.Network
{
    public class SidedRenderer : NetworkBehaviour
    {
        public bool visibleToOwner;
        public bool visibleToObservers;
        
        [Space]
        public bool includeChildren;

        private MeshRenderer[] renderers;

        private void Awake() { renderers = includeChildren ? GetComponentsInChildren<MeshRenderer>() : new[] { GetComponent<MeshRenderer>() }; }

        public override void OnNetworkSpawn() => UpdateRenderers();

        protected override void OnOwnershipChanged(ulong previous, ulong current) => UpdateRenderers();

        private void UpdateRenderers()
        {
            var visible = IsOwner ? visibleToOwner : visibleToObservers;

            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = visible ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}