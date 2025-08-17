using System;
using Runtime.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Enemies
{
    public class EnemyMotor : NetworkBehaviour
    {
        public float moveSpeed;
        public float moveAcceleration;
        public float turnSpeed;
        public float cornerThreshold = 0.5f;

        private int pathIndex;
        private NavMeshPath path;
        private Vector3 position;
        private Vector3 lastPosition;
        private float pathUpdateTimer;

        [HideInInspector] public Vector3 velocity;
        
        public Vector3 moveDirection { get; private set; }
        public NetworkObject target { get; private set; }

        private void Awake() { path = new NavMeshPath(); }

        private void OnEnable()
        {
            position = transform.position;
            lastPosition = position;
        }

        private void Update() { transform.position = Vector3.Lerp(lastPosition, position, (Time.time - Time.fixedTime) / Time.fixedDeltaTime); }

        private void FixedUpdate()
        {
            lastPosition = position;

            if (pathUpdateTimer > 0.5f)
            {
                pathUpdateTimer %= 0.5f;
                UpdatePath();
            }

            pathUpdateTimer += Time.deltaTime;

            moveDirection = Vector3.zero;
            if (target != null)
            {
                if (pathIndex < path.corners.Length)
                {
                    var corner = path.corners[pathIndex];
                    if ((corner - position).magnitude < cornerThreshold)
                    {
                        pathIndex++;
                    }
                    else
                    {
                        moveDirection = (corner - position).normalized;
                    }
                }
            }
            else
            {
                var best = (PlayerInput)null;
                var bestScore = float.MinValue;

                for (var i = 0; i < PlayerInput.players.Count; i++)
                {
                    var player = PlayerInput.players[i];
                    var score = -(player.transform.position - position).magnitude;
                    if (score > bestScore)
                    {
                        best = player;
                        bestScore = score;
                    }
                }

                target = best != null ? best.NetworkObject : null;
            }

            if (moveDirection.magnitude > 0.5f)
            {
                var forward = transform.forward;
                forward = Vector3.RotateTowards(forward, moveDirection, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, float.MaxValue);
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }

            var dot = Mathf.InverseLerp(0.7f, 1f, Vector3.Dot(transform.forward, moveDirection));
            velocity = Vector3.MoveTowards(velocity, moveDirection * dot * moveSpeed, moveSpeed * Mathf.Min(1f, Time.deltaTime / moveAcceleration));
            position += velocity * Time.deltaTime;
        }

        private void UpdatePath()
        {
            if (target != null)
            {
                NavMesh.SamplePosition(position, out var hit, 1f, -1);
                var target = this.target.transform.position;
                target += (transform.position - target).normalized;
                NavMesh.CalculatePath(hit.position, target, -1, path);
                pathIndex = 1;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (path != null && path.corners != null && pathIndex < path.corners.Length)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, path.corners[pathIndex]);
                Gizmos.color = Color.cyan;
                for (var i = pathIndex; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                }
            }
        }
    }
}