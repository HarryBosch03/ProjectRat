using System;
using Unity.Netcode;
using UnityEngine;

namespace Runtime.Player
{
    public class PlayerMotor : NetworkBehaviour
    {
        public float moveSpeed = 8f;
        public float acceleration = 0.15f;
        public float radius = 0.3f;

        [Space] public float jumpHeight = 1f;
        public int jumpLeniency = 8;

        private Vector3 deltaVelocity;
        private Collider[] collision;
        private Collider[] collisionBuffer = new Collider[32];
        private int jumpFrames;
        private int jumpInput;
        private Vector3 groundVelocityPrev;

        private Rigidbody ground;

        [HideInInspector] public Vector3 position;
        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public Vector2 rotation;

        public bool onGround { get; private set; }

        private Vector3 lastPosition;

        public void Jump() => jumpInput = jumpLeniency;

        private void OnEnable()
        {
            collision = GetComponentsInChildren<Collider>();
        }

        private void FixedUpdate()
        {
            lastPosition = position;
            transform.position = position;

            if (IsOwner)
            {
                Move();
                DoJump();
            }

            if (ground != null)
            {
                var groundVelocity = ground.GetPointVelocity(transform.position);
                var groundDeltaV = groundVelocity - groundVelocityPrev;
                deltaVelocity += groundDeltaV;
                
                groundVelocityPrev = groundVelocity;
            }
            
            Integrate();
            CollisionCheck();

            rotation.x %= 360f;
            rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
            transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);

            
            if (IsOwner)
            {
                SendNetStateRpc(position, velocity, rotation);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SendNetStateRpc(Vector3 position, Vector3 velocity, Vector2 rotation)
        {
            if (IsOwner) return;

            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
        }

        private void Update()
        {
            transform.position = Vector3.LerpUnclamped(lastPosition, position,
                (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        }

        private void DoJump()
        {
            jumpFrames--;

            if (jumpInput > 0 && onGround)
            {
                var jumpForce = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
                velocity.y = jumpForce;
                jumpFrames = 3;
            }

            jumpInput--;
        }

        private void Move()
        {
            if (onGround)
            {
                var target = Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed;
                var refFrame = onGround && ground != null ? ground.GetPointVelocity(transform.position) : Vector3.zero;

                var deltaVelocity =
                    Vector3.MoveTowards(velocity, refFrame + target, Time.deltaTime * moveSpeed / acceleration) -
                    velocity;
                deltaVelocity.y = 0f;

                this.deltaVelocity += deltaVelocity;
            }
        }

        private void CollisionCheck()
        {
            var castDistance = 1f;

            onGround = false;

            if (jumpFrames <= 0)
            {
                if (Physics.SphereCast(new Ray(position + Vector3.up * castDistance, Vector3.down), radius, out var hit,
                        castDistance - radius + 0.05f))
                {
                    position += Vector3.Project(hit.point - position, Vector3.up);
                    transform.position = position;

                    velocity.y = Mathf.Max(0f, velocity.y);

                    onGround = true;
                    ground = hit.rigidbody;
                }
            }

            foreach (var collider in collision)
            {
                if (collider.isTrigger) continue;

                var bounds = collider.bounds;
                var count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.size, collisionBuffer, Quaternion.identity,
                    ~0, QueryTriggerInteraction.Ignore);
                for (var i = 0; i < count; i++)
                {
                    var other = collisionBuffer[i];
                    if (other.transform.IsChildOf(transform)) continue;

                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation,
                            other, other.transform.position, other.transform.rotation, out var normal, out var depth))
                    {
                        position += normal * depth;
                        transform.position = position;

                        velocity += normal * Mathf.Max(0f, Vector3.Dot(normal, -velocity));
                    }
                }
            }
        }

        private void Integrate()
        {
            position += velocity * Time.fixedDeltaTime;
            velocity += deltaVelocity;
            deltaVelocity = Physics.gravity * Time.fixedDeltaTime;

            transform.position = position;
        }
    }
}