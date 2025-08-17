using System;
using UnityEngine;

namespace Runtime.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : MonoBehaviour
    {
        private const float ReferenceFieldOfView = 60f;
        
        public float fieldOfView = 90f;
        
        private UnityEngine.Camera mainCamera;
        private UnityEngine.Camera viewportCamera;

        public static float zoom { get; set; }
        public static float aimBlend { get; set; }
        
        private void Awake()
        {
            mainCamera = GetComponent<UnityEngine.Camera>();
            viewportCamera = transform.GetChild(0).GetComponent<UnityEngine.Camera>();
        }

        private void Update()
        {
            var aimedFov = Mathf.Atan(Mathf.Tan(ReferenceFieldOfView * 0.5f * Mathf.Deg2Rad) / zoom) * 2f * Mathf.Rad2Deg;
            mainCamera.fieldOfView = Mathf.Lerp(fieldOfView, aimedFov, aimBlend);
        }
    }
}