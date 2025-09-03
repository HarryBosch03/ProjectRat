using System;
using Runtime.Interactables;
using UnityEngine;

namespace Runtime.Train
{
    public class TrainControls : MonoBehaviour
    {
        public AnalogLever throttle;
        public float maxSpeedKmpH;
        public float acceleration;

        private Carriage motor;

        public float targetSpeed => throttle.normalizedValue * maxSpeedKmpH / 3.6f; 
        
        private void Awake()
        {
            motor = GetComponent<Carriage>();
        }

        private void FixedUpdate()
        {
            var targetSpeed = this.targetSpeed;
            if (targetSpeed > motor.forwardSpeed) motor.velocity += motor.transform.forward * (targetSpeed - motor.forwardSpeed) * Time.deltaTime * acceleration / maxSpeedKmpH * 2f;
        }
    }
}