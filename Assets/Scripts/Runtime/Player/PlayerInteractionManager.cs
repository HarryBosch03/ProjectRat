using System;
using Runtime.Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerInteractionManager : NetworkBehaviour
    {
        public float interactionRange = 3f;
        public Vector3 dropPosition;
        public Vector3 dropVelocity;

        private IInteractable lookingAt;
        
        public PlayerMotor motor { get; private set; }
        public HeldItem holding { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            lookingAt = GetLookingAt();

            if (IsOwner)
            {
                var kb = Keyboard.current;
                if (kb.fKey.wasPressedThisFrame)
                {
                    if (holding != null)
                    {
                        holding.Drop(this, motor.head.TransformPoint(dropPosition), motor.totalVelocity + motor.head.TransformVector(dropVelocity));
                    }
                    else
                    {
                        if (lookingAt != null)
                        {
                            lookingAt.Interact(this);
                        }
                    }
                }
            }
        }

        private IInteractable GetLookingAt()
        {
            var ray = new Ray(motor.head.position, motor.head.forward);
            if (Physics.Raycast(ray, out var hit, interactionRange))
            {
                return hit.collider.GetComponentInParent<IInteractable>();
            }

            return null;
        }

        [Rpc(SendTo.Everyone)]
        public void SetHoldingRpc(NetworkBehaviourReference heldItemRef)
        {
            heldItemRef.TryGet(out HeldItem heldItem);
            holding = heldItem;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.orange;
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f, 1.7f, 0f));
            Gizmos.DrawSphere(dropPosition, 0.05f);
        }
    }
}