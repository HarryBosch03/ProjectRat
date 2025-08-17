using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    public class PlayerInput : NetworkBehaviour
    {
        public float mouseSensitivity = 0.3f;

        private PlayerMotor motor;
        private UnityEngine.Camera mainCamera;

        public bool isActiveViewer => activeViewer == this;
        
        public static PlayerInput activeViewer { get; private set; }
        public static List<PlayerInput> players { get; } = new List<PlayerInput>();
        
        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            mainCamera = UnityEngine.Camera.main;
        }

        private void OnEnable()
        {
            players.Add(this);
        }

        private void OnDisable()
        {
            players.Remove(this);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked;
                activeViewer = this;
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

                var cameraTanLength = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                var deltaLook = Vector2.zero;
                deltaLook += m.delta.ReadValue() * mouseSensitivity * cameraTanLength;
                motor.rotation += deltaLook;
                motor.rotation.y = Mathf.Clamp(motor.rotation.y, -90f, 90f);
            }
        }

        private void LateUpdate()
        {
            if (isActiveViewer)
            {
                mainCamera.transform.SetPositionAndRotation(motor.head.position, motor.head.rotation);
            }
        }
    }
}