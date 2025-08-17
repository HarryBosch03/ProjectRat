using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Runtime.Enemies
{
    public class EnemyLegs : MonoBehaviour
    {
        public Rig ikRig;
        public float spacingAngle = 30f;
        public float placementRadius;
        public float footPlacementRadius;
        public float legMovementSpeed;
        public float legLeadTime;
        public float maxLegLiftHeight = 0.5f;
        public AnimationCurve liftCurve = new AnimationCurve(new []{ new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)});

        private EnemyMotor motor;
        
        private Vector3 legCenter;
        private readonly List<Leg> legs = new List<Leg>();

        private void Awake()
        {
            motor = GetComponent<EnemyMotor>();
            
            var legList = ikRig.GetComponentsInChildren<TwoBoneIKConstraint>();
            foreach (var ik in legList)
            {
                var leg = new Leg();
                var data = ik.data;
                
                leg.hint = data.hint;
                leg.target = data.target;
                leg.root = data.root;
                leg.side = ik.name[^1];
                leg.index = ik.name[^3] - '0';
                leg.adjacentLegs = new List<int>();

                leg.footPositionOnGround = leg.target.position;
                leg.footTarget = leg.target.position;
                leg.lastFootTarget = leg.target.position;
                
                legs.Add(leg);

                legCenter += data.root.position;
            }

            for (var i = 0; i < legs.Count; i++)
            {
                var leg = legs[i];
                
                for (var j = 0; j < legs.Count; j++)
                {
                    if (i == j) continue;
                    var other = legs[j];

                    if (other.side != leg.side && other.index == leg.index) leg.adjacentLegs.Add(j);
                    else if (other.side == leg.side && Mathf.Abs(other.index - leg.index) == 1) leg.adjacentLegs.Add(j);
                }
            }

            legCenter /= legList.Length;
            legCenter = transform.InverseTransformPoint(legCenter);
            legCenter.y = 0f;
        }

        private void Update()
        {
            for (var i = 0; i < legs.Count; i++)
            {
                var leg = legs[i];
                
                var angle = (leg.index - 2) * spacingAngle;
                if (leg.side == 'L') angle = 180f - angle;

                var basisPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, -Mathf.Sin(angle * Mathf.Deg2Rad)) * placementRadius;
                basisPosition = transform.TransformPoint(legCenter + basisPosition);

                var isAdjacentLegMoving = false;
                for (var j = 0; j < leg.adjacentLegs.Count; j++)
                {
                    if (legs[leg.adjacentLegs[j]].moving)
                    {
                        isAdjacentLegMoving = true;
                        break;
                    }
                }
                
                if (leg.moving)
                {
                    leg.footTarget = basisPosition + motor.velocity * legLeadTime;
                    var vector = leg.footTarget - leg.root.position;
                    if (Physics.Raycast(leg.root.position, vector, out var hit, vector.magnitude * 1.5f))
                    {
                        leg.footTarget = hit.point;
                    }
                    
                    leg.footPositionOnGround = transform.TransformPoint(Vector3.MoveTowards(transform.InverseTransformPoint(leg.footPositionOnGround), transform.InverseTransformPoint(leg.footTarget), Time.deltaTime * legMovementSpeed));
                    if ((leg.footPositionOnGround - leg.footTarget).magnitude < 0.01f) leg.moving = false;
                }
                else
                {
                    if ((leg.footPositionOnGround - basisPosition).magnitude > footPlacementRadius && !isAdjacentLegMoving)
                    {
                        leg.lastFootTarget = leg.footTarget;
                        leg.moving = true;
                    }
                    
                    leg.footPositionOnGround = leg.footTarget;
                }

                var lift = 1f - Vector3.Dot(leg.footPositionOnGround - leg.lastFootTarget, (leg.footTarget - leg.lastFootTarget).normalized) / (leg.footTarget - leg.lastFootTarget).magnitude;
                if (float.IsNaN(lift)) lift = 0f;
                    
                leg.target.position = leg.footPositionOnGround + transform.up * liftCurve.Evaluate(lift) * maxLegLiftHeight;
                leg.hint.position = leg.target.position + transform.up;

                legs[i] = leg;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                foreach (var leg in legs)
                {
                    Gizmos.matrix = Matrix4x4.TRS(leg.target.position, Quaternion.identity,  new Vector3(1f, 0f, 1f));
                    Gizmos.DrawWireSphere(Vector3.zero, footPlacementRadius);

                    Gizmos.matrix = Matrix4x4.identity;
                    
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(leg.footPositionOnGround, 0.01f);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(leg.footTarget, 0.01f);
                }
            }
        }

        public struct Leg
        {
            public TwoBoneIKConstraint ikConstraint;
            public Transform hint;
            public Transform target;
            public Transform root;
            
            public int index;
            public char side;

            public Vector3 footTarget;
            public Vector3 lastFootTarget;
            public Vector3 footPositionOnGround;
            public bool moving;

            public List<int> adjacentLegs;
        }
    }
}