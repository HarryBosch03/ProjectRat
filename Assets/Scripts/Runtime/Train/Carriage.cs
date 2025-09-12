using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Runtime.Train
{
    [RequireComponent(typeof(Rigidbody))]
    public class Carriage : MonoBehaviour
    {
        public SplineContainer currentSpline;
        public Vector3 velocity;
        public float drag;

        public Transform bogieFront;
        public Transform bogieBack;
        public Transform attachFront;
        public Transform attachBack;

        public Carriage connectionFront;
        public Carriage connectionBack;
        public float connectionDistance = 0f;

        private Rigidbody body;
        private Vector3? oldPosition;
        private Quaternion? oldRotation;

        public float mass => body.mass;
        public float totalMass { get; private set; }

        public float forwardSpeed => Vector3.Dot(transform.forward, velocity);

        public void AddForce(float force)
        {
            velocity += transform.forward * (force / totalMass * Time.deltaTime);
        }

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

        private void Start()
        {
            totalMass = GetTotalMass();
        }

        private void FixedUpdate()
        {
            totalMass = GetTotalMass();

            foreach (var c in GetCarriages())
            {
                c.StartModifyPose();
                c.ApplyForces();
            }

            for (var i = 0; i < 3; i++)
            {
                foreach (var c in GetCarriages())
                {
                    if (connectionBack != null)
                    {
                        c.ApplyConstraints();
                    }
                }
            }

            foreach (var c in GetCarriages())
                c.EndModifyPose();
        }

        private float GetTotalMass()
        {
            var head = this;
            var mass = 0f;
            while (head != null)
            {
                mass += head.body.mass;
                head = head.connectionBack;
            }

            return mass;
        }

        public void EndModifyPose()
        {
            if (!oldPosition.HasValue || !oldRotation.HasValue)
                throw new Exception("StartModifyPose must be called before EndModifyPose");

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

        public void ApplyForces()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;

            velocity *= 1f - (drag * Time.deltaTime);
        }

        public void ApplyConstraints()
        {
            if (connectionFront != null)
            {
                var otherAttach = connectionFront.attachBack;
                var attach = attachFront;

                var displacement = (otherAttach.position - attach.position) +
                                   (attach.position - otherAttach.position).normalized * connectionDistance;

                attach.LookAt(otherAttach.position, transform.up);
                otherAttach.LookAt(attach.position, connectionFront.transform.up);

                transform.position += displacement;
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

        public IEnumerable<Carriage> GetCarriages() => new CarriageEnumerator(this);

        public class CarriageEnumerator : IEnumerable<Carriage>
        {
            private Carriage root;

            public CarriageEnumerator(Carriage root)
            {
                this.root = root;
            }

            public IEnumerator<Carriage> GetEnumerator()
            {
                var head = root;
                while (head != null)
                {
                    yield return head;
                    head = head.connectionBack;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}