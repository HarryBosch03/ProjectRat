using System;
using UnityEngine;

namespace Runtime.Player
{
    public class StupidPlayerModel : MonoBehaviour
    {
        public Transform head;

        private PlayerMotor motor;

        private void Awake()
        {
            motor = GetComponentInParent<PlayerMotor>();
        }

        private void Update()
        {
            head.transform.rotation = Quaternion.Euler(-motor.rotation.y, motor.rotation.x, 0f);
        }
    }
}
