using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Network
{
    public class NetworkGUI : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Update()
        {
            var netManager = NetworkManager.Singleton;
            if (netManager != null && !netManager.IsServer && !netManager.IsClient)
            {
                var kb = Keyboard.current;
                if (kb.spaceKey.wasPressedThisFrame || kb.hKey.wasPressedThisFrame) netManager.StartHost();
                if (kb.cKey.wasPressedThisFrame) netManager.StartClient();
            }
        }

        private void OnGUI()
        {
            var netManager = NetworkManager.Singleton;
            if (netManager != null && !netManager.IsServer && !netManager.IsClient)
            {
                using (new GUILayout.AreaScope(new Rect(0, 0, 200, Screen.height)))
                {
                    if (GUILayout.Button("Start Host (H)")) netManager.StartHost();
                    if (GUILayout.Button("Start Client (C)")) netManager.StartClient();
                }
            }
        }
#endif
    }
}