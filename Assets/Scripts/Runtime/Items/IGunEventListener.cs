using UnityEngine;

namespace Runtime.Items
{
    public interface IGunEventListener
    {
        void OnShoot();
        void OnReloadStart();
        void OnReloadEnd();
        void OnHit(RaycastHit hit);
    }
}