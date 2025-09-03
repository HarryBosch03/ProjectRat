using UnityEngine;

namespace Runtime.Items
{
    public interface IItemHolder
    {
        Transform itemParent { get; }
        bool IsOwner { get; }

        void SetHoldingRpc(HeldItem item);
    }
}