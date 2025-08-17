using System;
using TMPro;
using UnityEngine;

namespace Runtime.Items
{
    [DefaultExecutionOrder(10)]
    public class GunHud : MonoBehaviour, IGunEventListener, IHeldItemBehaviour
    {
        public Canvas canvas;
        public TMP_Text infoText;

        private Gun gun;

        private void Awake()
        {
            gun = GetComponentInParent<Gun>();
            canvas.gameObject.SetActive(enabled);
        }

        private void OnEnable()
        {
            UpdateInfoText();
            canvas.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            canvas.gameObject.SetActive(false);
        }

        private void UpdateInfoText()
        {
            var displayName = gun.heldItem.displayName;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "<Unnamed Item>";
            infoText.text = $"{displayName}\n{gun.currentMagazine}/{gun.magazineSize}";
        }

        public void OnShoot()
        {
            if (!isActiveAndEnabled) return;
            UpdateInfoText();
        }

        public void OnReloadStart()
        {
            if (!isActiveAndEnabled) return;
            UpdateInfoText();
        }

        public void OnReloadEnd()
        {
            if (!isActiveAndEnabled) return;
            UpdateInfoText();
        }

        public void OnHit(RaycastHit hit) { }
    }
}