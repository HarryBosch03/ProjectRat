using System;
using UnityEngine;

namespace Runtime.Items
{
    public class WeaponSway : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float strength;
        public float smoothing;

        private Quaternion rotation;
        
        private void Update()
        {
            var target = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            rotation = Quaternion.Slerp(rotation, target, Time.deltaTime / smoothing);
            transform.rotation = Quaternion.Slerp(target, rotation, strength);
        }
    }
}