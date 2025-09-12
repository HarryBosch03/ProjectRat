using System;
using Runtime.Items;
using Runtime.Player;
using Unity.Netcode;
using UnityEngine;

namespace Runtime.Interactables
{
    public class ItemRack : NetworkBehaviour, IInteractable
    {
        public HeldItem[] items;
        public Transform[] itemAnchors;

        private void Start()
        {
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                item.EnablePhysics = false;
                
                var localPos = Vector3.zero;
                var localRot = Quaternion.identity;

                if (i < itemAnchors.Length)
                {
                    var anchor = itemAnchors[i];
                    localPos = transform.InverseTransformPoint(anchor.position);
                    localRot = Quaternion.Inverse(transform.rotation) * anchor.rotation;
                }

                item.transform.SetParent(transform);
                item.transform.SetLocalPositionAndRotation(localPos, localRot);
            }
        }

        [field: SerializeField]
        public string InteractionText => GetNextItem().InteractionText;

        public HeldItem GetNextItem()
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].transform.parent == transform) return items[i];
            }

            return null;
        }

        public void Interact(PlayerInteractionManager player)
        {
            if (player.holding == null)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i].transform.parent != transform) continue;

                    items[i].gameObject.SetActive(true);
                    items[i].Interact(player);
                    return;
                }
            }
            else
            {
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (item != player.holding) continue;

                    var localPos = Vector3.zero;
                    var localRot = Quaternion.identity;

                    if (i < itemAnchors.Length)
                    {
                        var anchor = itemAnchors[i];
                        localPos = transform.InverseTransformPoint(anchor.position);
                        localRot = Quaternion.Inverse(transform.rotation) * anchor.rotation;
                    }

                    item.Store(player, NetworkObject, localPos, localRot);

                    return;
                }
            }
        }

        public void Nudge(PlayerInteractionManager player, int direction) { }
    }
}