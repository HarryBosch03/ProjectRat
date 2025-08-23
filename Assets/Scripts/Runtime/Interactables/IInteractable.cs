using Runtime.Player;

namespace Runtime.Interactables
{
    public interface IInteractable
    {
        void Interact(PlayerInteractionManager player);
        void Nudge(PlayerInteractionManager player, int direction);
    }
}