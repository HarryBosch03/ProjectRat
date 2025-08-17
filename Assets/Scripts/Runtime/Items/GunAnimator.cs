using System;
using Runtime.Utility;
using UnityEngine;

namespace Runtime.Items
{
    [RequireComponent(typeof(Animator))]
    public class GunAnimator : MonoBehaviour, IGunEventListener
    {
        private static readonly int ShootAnimParameter = Animator.StringToHash("shoot");
        private static readonly int ReloadAnimParameter = Animator.StringToHash("reload");
        private static readonly int MovementAnimParameter = Animator.StringToHash("movement");
        private static readonly int AimPercentAnimParameter = Animator.StringToHash("aim percent");
        
        private Animator animator;
        private Gun gun;
        private float smoothedMovement;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            gun = GetComponentInParent<Gun>(true);
        }

        private void Update()
        {
            var holder = gun.holder;
            if (holder != null)
            {
                var motor = holder.motor;
                var movement = motor.onGround ? new Vector2(motor.localVelocity.x, motor.localVelocity.z).magnitude / motor.moveSpeed : 0f;
                smoothedMovement = Mathf.Lerp(smoothedMovement, movement, Time.deltaTime / 0.1f);
                animator.SetFloat(MovementAnimParameter, smoothedMovement);
            }
            
            animator.SetFloat(AimPercentAnimParameter, Curves.Smootherstep(gun.aimPercent));
        }

        public void OnShoot()
        {
            if (!isActiveAndEnabled) return;
            animator.SetTrigger(ShootAnimParameter);
        }

        public void OnReloadStart()
        {
            if (!isActiveAndEnabled) return;
            animator.SetTrigger(ReloadAnimParameter);
        }

        public void OnReloadEnd()
        {
            
        }

        public void OnHit(RaycastHit hit)
        {
            
        }
    }
}