using Runtime.Player;
using UnityEngine;

namespace Runtime.Interactables
{
    public class AnalogLever : MonoBehaviour, IInteractable
    {
        public int currentValue;
        public int minValue;
        public int maxValue;

        [field: SerializeField]
        public string InteractionText { get; set; }
        public float normalizedValue => Mathf.InverseLerp(minValue, maxValue, currentValue);

        public void Interact(PlayerInteractionManager player)
        {
            
        }

        public void Nudge(PlayerInteractionManager player, int direction)
        {
            currentValue += direction;
            currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        }
    }
}