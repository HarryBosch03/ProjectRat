using TMPro;
using UnityEngine;

namespace Runtime.Train
{
    [RequireComponent(typeof(TMP_Text))]
    public class TrainDebugText : MonoBehaviour
    {
        private TrainMotor motor;
        private TrainControls controls;
        private TMP_Text text;

        private void Awake()
        {
            motor = GetComponentInParent<TrainMotor>();
            controls = GetComponentInParent<TrainControls>();
            text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            var txt = $"Speed: {(motor.forwardSpeed * 3.6f):N2}";
            if (controls != null) txt += $"\nTSpeed: {(controls.targetSpeed * 3.6f):N2}";
            
            text.text = txt;
        }
    }
}