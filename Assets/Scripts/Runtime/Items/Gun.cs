using System.Collections.Generic;
using Runtime.Camera;
using Runtime.Player;
using Runtime.Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Items
{
    [RequireComponent(typeof(HeldItem))]
    public class Gun : NetworkBehaviour, IHeldItemBehaviour
    {
        public float damage;
        public float fireRate;
        public float adsDuration = 0.15f;
        public float adsZoom = 1f;
        public bool singleFire;
        public int magazineSize;
        public int currentMagazine;
        public float reloadDuration;
        public float ammoCheckDuration;

        [Space]
        public float recoilStrength = 50f;
        public float recoilVariance = 10f;
        public float recoilAngle = 45f;
        public float recoilDamping = 20f;

        [Space]
        public Transform headBone;
        public Vector3 headBoneRotationCorrection = new Vector3(90f, 180f, 0f);

        private PlayerHud playerHud;

        private float nextShootTime;
        private float reloadTimer;
        private Vector2 recoilVelocity;

        public PlayerInteractionManager holder { get; private set; }
        public HeldItem heldItem { get; private set; }
        public float aimPercent { get; private set; }

        private readonly List<IGunEventListener> eventListeners = new List<IGunEventListener>();

        public bool isReloading => reloadTimer > 0f;
        public bool pauseInput => isReloading;

        private void Awake()
        {
            heldItem = GetComponentInParent<HeldItem>();
            eventListeners.AddRange(GetComponentsInChildren<IGunEventListener>(true));
        }

        private void OnEnable()
        {
            holder = heldItem.holder;
            holder.motor.headBone = headBone;
            holder.motor.headBoneRotationCorrection = headBoneRotationCorrection;

            playerHud = holder.GetComponent<PlayerHud>();

            reloadTimer = 0f;
        }

        private void OnDisable()
        {
            if (holder.motor.headBone == headBone)
            {
                holder.motor.headBone = null;
                holder = null;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                var kb = Keyboard.current;
                var m = Mouse.current;

                if (!pauseInput)
                {
                    if ((singleFire ? m.leftButton.wasPressedThisFrame : m.leftButton.isPressed) && Time.time > nextShootTime)
                    {
                        if (currentMagazine > 0)
                        {
                            var head = holder.motor.head;
                            ShootRpc(head.position, head.forward);
                        }
                        // else
                        // {
                        //     Reload();
                        // }
                    }

                    aimPercent = Mathf.MoveTowards(aimPercent, m.rightButton.isPressed ? 1 : 0, Time.deltaTime / adsDuration);

                    if (!isReloading && kb.rKey.isPressed)
                    {
                        ReloadRpc();
                    }
                }
                else
                {
                    aimPercent = Mathf.MoveTowards(aimPercent, 0f, Time.deltaTime / adsDuration);
                }

                if (isReloading)
                {
                    reloadTimer -= Time.deltaTime;
                    if (reloadTimer <= 0f)
                    {
                        currentMagazine = magazineSize;
                        for (var i = 0; i < eventListeners.Count; i++) eventListeners[i].OnReloadEnd();
                    }
                }

                CameraController.zoom = adsZoom;
                CameraController.aimBlend = Curves.Smootherstep(aimPercent);
                if (playerHud != null)
                {
                    playerHud.centerDot.alpha = 1f - Curves.Smootherstep(aimPercent);
                }

                holder.motor.rotation += recoilVelocity * Time.deltaTime;
                recoilVelocity -= recoilVelocity * recoilDamping * Time.deltaTime;

                SyncStateRpc(aimPercent);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SyncStateRpc(float aimPercent)
        {
            if (IsOwner) return;

            this.aimPercent = aimPercent;
        }

        [Rpc(SendTo.Everyone)]
        private void ReloadRpc()
        {
            currentMagazine = 0;
            reloadTimer = reloadDuration;
            
            for (var i = 0; i < eventListeners.Count; i++) eventListeners[i].OnReloadStart();
        }

        [Rpc(SendTo.Everyone)]
        private void ShootRpc(Vector3 position, Vector3 direction)
        {
            var ray = new Ray(position, direction);
            if (Physics.Raycast(ray, out var hit))
            {
                for (var i = 0; i < eventListeners.Count; i++) eventListeners[i].OnHit(hit);
            }

            var recoilStrength = this.recoilStrength + Random.Range(-recoilVariance, recoilVariance) * 0.5f;
            var recoilAngle = Random.Range(-this.recoilAngle, this.recoilAngle) * 0.5f;
            recoilVelocity += new Vector2(Mathf.Sin(recoilAngle * Mathf.Deg2Rad), Mathf.Cos(recoilAngle * Mathf.Deg2Rad)) * recoilStrength;

            nextShootTime = Time.time + 60f / Mathf.Min(fireRate, 3600f);
            currentMagazine--;

            for (var i = 0; i < eventListeners.Count; i++) eventListeners[i].OnShoot();
        }

        public string GetItemInfo() { return $"{currentMagazine}/{magazineSize}"; }
    }
}