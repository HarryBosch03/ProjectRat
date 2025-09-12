using Runtime.Interactables;
using Runtime.Player;
using Unity.Netcode;
using UnityEngine;

namespace Runtime.Items
{
    public class HeldItem : NetworkBehaviour, IInteractable
    {
        public string displayName;

        public Rigidbody inWorldModel;
        public GameObject firstPersonHeldModel;
        public GameObject thirdPersonHeldModel;

        private UnityEngine.Camera mainCamera;
        private PlayerInput playerInput;

        private IHeldItemBehaviour[] heldBehaviours;

        public string InteractionText => $"Pickup {displayName}";
        public PlayerInteractionManager holder { get; private set; }

        public bool EnablePhysics
        {
            get => !inWorldModel.isKinematic;
            set
            {
                inWorldModel.interpolation = value ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
                inWorldModel.constraints = value ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
                inWorldModel.isKinematic = !value;
            }
        }

        private void Awake()
        {
            heldBehaviours = GetComponentsInChildren<IHeldItemBehaviour>(true);
            foreach (var heldBehaviour in heldBehaviours) heldBehaviour.enabled = false;
            firstPersonHeldModel.gameObject.SetActive(false);

            mainCamera = UnityEngine.Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                RequestHolderRpc();
            }
        }

        [Rpc(SendTo.Owner)]
        private void RequestHolderRpc(RpcParams rpcParams = default)
        {
            var sendParams = new RpcParams();
            sendParams.Send.Target = RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp);
            SetHolderRpc(holder, sendParams);
        }
        
        public void Interact(PlayerInteractionManager player)
        {
            if (holder != null) return;
            SetHolderRpc(player);
        }

        public void Nudge(PlayerInteractionManager player, int direction) { }

        public void Store(PlayerInteractionManager player, NetworkObject parent, Vector3 localPosition, Quaternion localRotation)
        {
            if (player != holder) return;
            SetHolderRpc(null);
            StoreRpc(parent, localPosition, localRotation);
        }

        [Rpc(SendTo.Everyone)]
        private void StoreRpc(NetworkObjectReference parentReference, Vector3 localPosition, Quaternion localRotation)
        {
            parentReference.TryGet(out var parent);
            
            transform.SetParent(parent.transform);
            transform.SetLocalPositionAndRotation(localPosition, localRotation);
            EnablePhysics = false;
        }

        public void Drop(PlayerInteractionManager player, Vector3 position, Vector3 velocity)
        {
            if (player != holder) return;
            SetHolderRpc(null);
            DropRpc(position, velocity);
        }

        [Rpc(SendTo.Everyone)]
        private void DropRpc(Vector3 position, Vector3 velocity)
        {
            EnablePhysics = true;
            
            inWorldModel.transform.position = position;
            inWorldModel.linearVelocity = velocity;
            
            Physics.SyncTransforms();
        }

        private void Update()
        {
            if (holder != null)
            {
                var isFirstPerson = playerInput.isActiveViewer;

                firstPersonHeldModel.SetActive(isFirstPerson);
                thirdPersonHeldModel.SetActive(!isFirstPerson);
            }
        }

        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetHolderRpc(NetworkBehaviourReference playerRef, RpcParams rpcParams = default)
        {
            playerRef.TryGet(out PlayerInteractionManager player);

            var previousHolder = holder;
            if (player != null && player.holding == null)
            {
                holder = player;
                
                inWorldModel.gameObject.SetActive(false);

                foreach (var heldBehaviour in heldBehaviours) heldBehaviour.enabled = true;

                transform.SetParent(player.motor.head);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                inWorldModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                inWorldModel.linearVelocity = Vector3.zero;
                inWorldModel.angularVelocity = Vector3.zero;

                if (player.IsOwner) player.SetHoldingRpc(this);

                if (IsServer)
                {
                    NetworkObject.ChangeOwnership(player.OwnerClientId);
                }

                playerInput = player.GetComponent<PlayerInput>();
            }
            else
            {
                holder = null;
                
                inWorldModel.gameObject.SetActive(true);
                firstPersonHeldModel.SetActive(false);
                thirdPersonHeldModel.SetActive(false);

                foreach (var heldBehaviour in heldBehaviours) heldBehaviour.enabled = false;

                transform.SetParent(null);

                if (previousHolder != null && previousHolder.IsOwner) previousHolder.SetHoldingRpc(null);

                if (IsServer)
                {
                    NetworkObject.RemoveOwnership();
                }

                playerInput = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (var heldBehaviour in GetComponentsInChildren<IHeldItemBehaviour>(true))
                {
                    heldBehaviour.enabled = false;
                }
            }
        }
#endif
    }
}