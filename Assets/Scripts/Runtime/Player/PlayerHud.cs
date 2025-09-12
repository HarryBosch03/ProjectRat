using System;
using Runtime.Interactables;
using TMPro;
using UnityEngine;

namespace Runtime.Player
{
    public class PlayerHud : MonoBehaviour
    {
        public CanvasGroup centerDot;
        public TMP_Text interactionText;

        private PlayerInteractionManager interactionManager;
        private IInteractable lookingAt;

        private void Awake()
        {
            interactionManager = GetComponent<PlayerInteractionManager>();
        }

        private void OnEnable()
        {
            UpdateLookingAtText(lookingAt);
        }

        private void Update()
        {
            var lookingAt = interactionManager.holding == null ? interactionManager.lookingAt : null;
            if (this.lookingAt != lookingAt)
            {
                UpdateLookingAtText(lookingAt);
            }
        }

        private void UpdateLookingAtText(IInteractable lookingAt)
        {
            this.lookingAt = lookingAt;
            interactionText.text = lookingAt != null ? this.lookingAt.InteractionText : string.Empty;
        }
    }
}