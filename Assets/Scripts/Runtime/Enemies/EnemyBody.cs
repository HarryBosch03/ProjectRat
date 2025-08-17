using System;
using UnityEngine;

namespace Runtime.Enemies
{
    public class EnemyBody : MonoBehaviour
    {
        public Transform head;
        public Transform middle;
        public Transform abdomen;
        public float tilt;
        public float maxTiltAngle = 30f;
        
        private EnemyMotor motor;

        private void Awake()
        {
            motor = GetComponent<EnemyMotor>();
        }

        private void Update()
        {
            var current = motor.transform.forward;
            var target = motor.target != null ? (motor.target.transform.position - motor.transform.position).normalized : current;

            var yaw = Mathf.Clamp(Vector3.SignedAngle(current, target, motor.transform.up) * tilt, -maxTiltAngle, maxTiltAngle);
            var pitch = 0f;
            if (motor.target != null)
            {
                var difference = motor.target.transform.position - head.transform.position + Vector3.up * 1.6f;
                pitch = Mathf.Asin(Mathf.Clamp(difference.y / new Vector2(difference.x, difference.z).magnitude, -1f, 1f)) * Mathf.Rad2Deg;
                pitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);
            }
            
            head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            abdomen.localRotation = Quaternion.Euler(0f, 180f - yaw, 0f);
        }
    }
}