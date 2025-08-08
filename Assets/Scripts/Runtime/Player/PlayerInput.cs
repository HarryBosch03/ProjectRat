using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    public class PlayerInput : NetworkBehaviour
    {
        public float mouseSensitivity = 0.3f;

        private PlayerMotor motor;
        private Camera mainCamera;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            mainCamera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                var kb = Keyboard.current;
                var m = Mouse.current;

                var moveInput = new Vector2
                {
                    x = kb.dKey.ReadValue() - kb.aKey.ReadValue(),
                    y = kb.wKey.ReadValue() - kb.sKey.ReadValue(),
                };

                motor.moveDirection = motor.transform.TransformVector(moveInput.x, 0f, moveInput.y);

                if (kb.spaceKey.wasPressedThisFrame) motor.Jump();

                var deltaLook = Vector2.zero;
                deltaLook += m.delta.ReadValue() * mouseSensitivity;
                motor.rotation += deltaLook;
                motor.rotation.y = Mathf.Clamp(motor.rotation.y, -90f, 90f);
            }
        }

        private void LateUpdate()
        {
            if (IsOwner)
            {
                mainCamera.transform.position = motor.transform.position + Vector3.up * 1.7f;

                var rotation = motor.rotation;
                mainCamera.transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);
            }
        }
    }
}