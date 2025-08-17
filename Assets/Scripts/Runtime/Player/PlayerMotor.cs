using System.Collections.Generic;
using Runtime.Utility;
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
        public int jumpLeniencyFrames = 8;
        public float stepHeight = 0.5f;

        [Space]
        public float ladderClimbSpeed = 3f;
        public float ladderSmoothing = 0.1f;

        [Space]
        public Transform head;

        private Vector3 deltaVelocity;
        private Collider[] collision;
        private Collider[] collisionBuffer = new Collider[32];
        private int jumpFrames;
        private int jumpInput;

        private Rigidbody ground;
        private Vector3 positionOnGround;
        private float lastGroundRotation;
        private float ladderPos;
        
        private Ladder ladder;
        private readonly List<Collider> currentTriggers = new List<Collider>(64);
        private readonly List<Collider> currentTriggersBuffer = new List<Collider>(64);

        [HideInInspector] public Vector3 position;
        [HideInInspector] public Vector3 localVelocity;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public Vector2 rotation;

        public bool onGround { get; private set; }
        public Transform headBone { get; set; }
        public Vector3 headBoneRotationCorrection { get; set; }
        
        public Vector3 totalVelocity => localVelocity + (ground != null ? ground.GetPointVelocity(position) : Vector3.zero);

        private Vector3 lastPosition;

        public void Jump() => jumpInput = jumpLeniencyFrames;

        private void OnEnable() { collision = GetComponentsInChildren<Collider>(); }

        private void FixedUpdate()
        {
            lastPosition = position;
            if (ground != null) 
                position = ground.transform.TransformPoint(positionOnGround);
            transform.position = position;
            
            if (IsOwner)
            {
                if (ladder != null)
                {
                    DoLadderStuff();
                }
                else
                {
                    Move();
                    DoJump();
                }
                
                jumpInput--;
            }

            Integrate();
            if (ladder == null)
            {
                CollisionCheck();
            }

            if (ground != null) 
                positionOnGround = ground.transform.InverseTransformPoint(position);
           
            if (IsOwner) 
                SendNetStateRpc(position, localVelocity, rotation);
        }

        private void DoLadderStuff()
        {
            var sign = -Mathf.Sign(Vector3.Dot(transform.forward, ladder.transform.forward));
            ladderPos += Vector3.Dot(moveDirection, transform.forward) * sign * ladderClimbSpeed * Time.deltaTime;
            if (ladderPos > ladder.Length - 0.1f || ladderPos < 0.1f)
            {
                ladder = null;
                return;
            }
            
            if (jumpInput > 0)
            {
                var jumpForce = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
                var jumpVector = (ladder.transform.forward + Vector3.up).normalized;
                localVelocity = jumpVector * jumpForce;
                jumpFrames = 3;
                
                ladder = null;
            }

            var positionOnLadder = ladder.GetPosition(ladderPos);
            positionOnLadder += ladder.transform.forward * Mathf.Clamp(ladder.Length - ladderPos, 0f, 1f) * 0.5f;
            
            position = Vector3.Lerp(position, positionOnLadder, Time.deltaTime / ladderSmoothing);
            localVelocity = Vector3.zero;
        }

        [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Unreliable)]
        private void SendNetStateRpc(Vector3 position, Vector3 velocity, Vector2 rotation)
        {
            if (IsOwner) return;

            this.position = position;
            this.localVelocity = velocity;
            this.rotation = rotation;
        }

        private void Update()
        {
            transform.position = Vector3.LerpUnclamped(lastPosition, position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            if (ground != null)
            {
                var groundRotation = Vector3.SignedAngle(Vector3.forward, ground.transform.forward, Vector3.up);
                var deltaAngle = Mathf.DeltaAngle(groundRotation, lastGroundRotation);
                rotation.x -= deltaAngle;

                lastGroundRotation = groundRotation;
            }

            rotation.x %= 360f;
            rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);

            transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);
            
            head.localPosition = new Vector3(0f, 1.7f, 0f);
            head.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);
            
            if (headBone != null)
            {
                head.localPosition += headBone.localPosition;
                head.localRotation *= headBone.localRotation * Quaternion.Euler(headBoneRotationCorrection);
            }
        }

        private void DoJump()
        {
            jumpFrames--;

            if (jumpInput > 0 && onGround)
            {
                var jumpForce = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
                localVelocity.y = jumpForce;
                jumpFrames = 3;
            }
        }

        private void Move()
        {
            if (onGround)
            {
                var target = Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed;

                var deltaVelocity =
                    Vector3.MoveTowards(localVelocity, target, Time.deltaTime * moveSpeed / acceleration) -
                    localVelocity;
                deltaVelocity.y = 0f;

                this.deltaVelocity += deltaVelocity;
            }
        }

        private void CollisionCheck()
        {
            var castDistance = stepHeight + radius;
            var mask = PhysicsMatrix.GetCollisionMaskForLayer(gameObject.layer);
            var castExtension = onGround ? stepHeight : 0f; 

            onGround = false;

            if (jumpFrames <= 0)
            {
                if (Physics.SphereCast(new Ray(position + Vector3.up * castDistance, Vector3.down), radius, out var hit, castDistance - radius + castExtension + 0.05f, mask))
                {
                    position += Vector3.Project(hit.point - position, Vector3.up);
                    transform.position = position;

                    localVelocity.y = Mathf.Max(0f, localVelocity.y);

                    onGround = true;
                    if (ground != hit.rigidbody && hit.rigidbody != null)
                    {
                        lastGroundRotation = Vector3.SignedAngle(Vector3.forward, hit.rigidbody.transform.forward, Vector3.up);
                    }

                    ground = hit.rigidbody;
                }
            }

            currentTriggersBuffer.Clear();
            currentTriggersBuffer.AddRange(currentTriggers);
            currentTriggers.Clear();
            
            foreach (var collider in collision)
            {
                if (collider.isTrigger) continue;

                var bounds = collider.bounds;
                var count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.size, collisionBuffer, Quaternion.identity,
                    mask, QueryTriggerInteraction.Collide);
                
                for (var i = 0; i < count; i++)
                {
                    var other = collisionBuffer[i];
                    if (other.transform.IsChildOf(transform)) continue;
   
                    if (other.isTrigger)
                    {
                        if (IsOwner)
                        {
                            if (!currentTriggersBuffer.Contains(other))
                            {
                                TriggerEntered(other);
                            }
                        }

                        currentTriggers.Add(other);
                        continue;
                    }

                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation,
                            other, other.transform.position, other.transform.rotation, out var normal, out var depth))
                    {
                        position += normal * depth;
                        transform.position = position;

                        localVelocity += normal * Mathf.Max(0f, Vector3.Dot(normal, -localVelocity));
                    }
                }
            }
            
            foreach (var trigger in currentTriggersBuffer)
            {
                if (!currentTriggers.Contains(trigger)) TriggerExited(trigger);
            }
        }

        private void TriggerEntered(Collider trigger)
        {
            if (trigger.gameObject.CompareTag("Ladder") && trigger.TryGetComponent(out Ladder ladder))
            {
                this.ladder = ladder;
                ladderPos = Mathf.Clamp(ladder.GetPosition(position), 0.1f, ladder.Length - 0.1f);
            }
        }
        
        private void TriggerExited(Collider trigger)
        {
            
        }

        private void Integrate()
        {
            position += localVelocity * Time.fixedDeltaTime;
            localVelocity += deltaVelocity;
            deltaVelocity = ladder == null ? Physics.gravity * Time.fixedDeltaTime : Vector3.zero;

            transform.position = position;
        }
    }
}