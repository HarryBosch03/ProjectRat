using System;
using UnityEngine;

namespace Runtime.Utility
{
    public class Lockstepper : MonoBehaviour
    {
        private void Awake()
        {
            if (!enabled) OnDisable();
        }

        private void OnEnable()
        {
            Application.targetFrameRate = (int)(1f / Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            Application.targetFrameRate = -1;
        }
    }
}