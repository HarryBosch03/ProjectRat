using Runtime.Player;

namespace Runtime.Interactables
{
    public interface IInteractable
    {
        string InteractionText { get; }
        void Interact(PlayerInteractionManager player);
        void Nudge(PlayerInteractionManager player, int direction);
    }
}