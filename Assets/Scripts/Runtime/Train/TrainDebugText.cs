using TMPro;
using UnityEngine;

namespace Runtime.Train
{
    [RequireComponent(typeof(TMP_Text))]
    public class TrainDebugText : MonoBehaviour
    {
        private Carriage motor;
        private Locomotive controls;
        private TMP_Text text;

        private void Awake()
        {
            motor = GetComponentInParent<Carriage>();
            controls = GetComponentInParent<Locomotive>();
            text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            var txt = $"Speed: {(motor.forwardSpeed * 3.6f):N2}";
            if (controls != null) txt += $"\nThrottle: {(controls.throttleLever.normalizedValue * 100f):N0}%";
            
            text.text = txt;
        }
    }
}