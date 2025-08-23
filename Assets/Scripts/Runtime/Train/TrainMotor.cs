using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Runtime.Train
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrainMotor : MonoBehaviour
    {
        public SplineContainer currentSpline;
        public Vector3 velocity;
        public Transform wheelAnchor0;
        public Transform wheelAnchor1;

        private Rigidbody body;
        
        public float forwardSpeed => Vector3.Dot(transform.forward, velocity);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var oldPosition = transform.position;
            var oldRotation = transform.rotation;
            
            DoPhysicsStuff();

            var newPosition = transform.position;
            var newRotation = transform.rotation;

            transform.position = oldPosition;
            transform.rotation = oldRotation;

            body.MovePosition(newPosition);
            body.MoveRotation(newRotation);
        }

        private void DoPhysicsStuff()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;
            
            AlignToTracks();
        }

        private void AlignToTracks()
        {
            SplineUtility.GetNearestPoint(currentSpline.Spline, wheelAnchor0.position, out var nearest0, out var t0);
            transform.position += (Vector3)nearest0 - wheelAnchor0.position;
        
            SplineUtility.GetNearestPoint(currentSpline.Spline, wheelAnchor1.position, out var nearest1, out var t1);
            var direction = nearest0 - nearest1;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.position += (Vector3)nearest0 - wheelAnchor0.position;

            velocity -= Vector3.ProjectOnPlane(velocity, transform.forward);
        }
    }
}
