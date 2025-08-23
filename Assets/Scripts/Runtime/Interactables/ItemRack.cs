using System;
using Runtime.Items;
using Runtime.Player;
using UnityEngine;

namespace Runtime.Interactables
{
    public class ItemRack : MonoBehaviour, IInteractable
    {
        public HeldItem[] items;

        private void Start()
        {
            for (var i = 0; i < items.Length; i++) items[i].EnablePhysics = false;
        }

        [field: SerializeField]
        public string InteractionText { get; set; }

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

                    item.Drop(player, transform.position, Vector3.zero, false);
                    item.transform.SetParent(transform);
                    
                    return;
                }
            }
        }

        public void Nudge(PlayerInteractionManager player, int direction) { }
    }
}