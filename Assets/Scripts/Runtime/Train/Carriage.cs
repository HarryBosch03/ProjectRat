using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Runtime.Train
{
    [RequireComponent(typeof(Rigidbody))]
    public class Carriage : MonoBehaviour
    {
        public SplineContainer currentSpline;
        public Vector3 velocity;

        public Transform bogieFront;
        public Transform bogieBack;
        public Transform attachFront;
        public Transform attachBack;
        
        public Carriage connectionFront;
        public Carriage connectionBack;

        private Rigidbody body;
        private Vector3? oldPosition;
        private Quaternion? oldRotation;
        
        public float forwardSpeed => Vector3.Dot(transform.forward, velocity);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            
            if (connectionFront == null)
            {
                var current = this;
                while (current != null)
                {
                    current.AlignToTracks();
                    current = current.connectionBack;
                }
            }
            
            enabled = connectionFront == null;
        }

        private void FixedUpdate()
        {
            StartModifyPose();
            DoPhysicsStuff();
            EndModifyPose();

            if (connectionBack != null)
            {
                connectionBack.FixedUpdate();
            }
        }

        public void EndModifyPose()
        {
            if (!oldPosition.HasValue || !oldRotation.HasValue) throw new Exception("StartModifyPose must be called before EndModifyPose");
            
            var newPosition = transform.position;
            var newRotation = transform.rotation;

            transform.position = oldPosition.Value;
            transform.rotation = oldRotation.Value;

            body.MovePosition(newPosition);
            body.MoveRotation(newRotation);
        }

        public void StartModifyPose()
        {
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }

        public void DoPhysicsStuff()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;

            if (connectionFront != null)
            {
                var otherAttach = connectionFront.attachBack;
                var attach = attachFront;

                transform.position += (otherAttach.position - attach.position);
            }
            
            AlignToTracks();
        }

        private void AlignToTracks()
        {
            SplineUtility.GetNearestPoint(currentSpline.Spline, bogieFront.position, out var nearest0, out var t0);
            transform.position += (Vector3)nearest0 - bogieFront.position;
        
            SplineUtility.GetNearestPoint(currentSpline.Spline, bogieBack.position, out var nearest1, out var t1);
            var direction = nearest0 - nearest1;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.position += (Vector3)nearest0 - bogieFront.position;

            velocity -= Vector3.ProjectOnPlane(velocity, transform.forward);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (connectionBack != null && connectionBack.connectionFront != this)
                {
                    connectionBack.connectionFront = this;
                }
            }
        }
    }
}
