using System;
using Runtime.Attributes;
using Runtime.Interactables;
using UnityEngine;

namespace Runtime.Train
{
    public class Locomotive : MonoBehaviour
    {
        public AnalogLever throttleLever;
        [KmpH] public float enginePowerMaxSpeed;
        [KmpH] public float engineMaxPower;
        public AnimationCurve enginePowerCurve;
        public float throttleSmoothing;

        private Carriage motor;
        public float SmoothedThrottle { get; private set; }
        public float engineVelocity => Vector3.Dot(motor.transform.forward, motor.velocity);

        private void Awake()
        {
            motor = GetComponent<Carriage>();
        }

        private void FixedUpdate()
        {
            var throttle = throttleLever.normalizedValue;
            SmoothedThrottle = Mathf.Lerp(SmoothedThrottle, throttle, Time.deltaTime / Mathf.Max(Time.deltaTime, throttleSmoothing));
            
            var engineVelocity = this.engineVelocity;
            var engineSpeed = Mathf.Abs(engineVelocity);

            var enginePower = enginePowerCurve.Evaluate(engineSpeed / enginePowerMaxSpeed) * engineMaxPower;
            enginePower *= throttle != null ? SmoothedThrottle : 0f;

            motor.AddForce(enginePower * motor.mass);
        }
    }
}