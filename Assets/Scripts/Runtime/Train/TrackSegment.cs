using System.Collections.Generic;
using System.Diagnostics;
using Runtime.Utility;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Runtime.Train
{
    public class TrackSegment : MonoBehaviour
    {
        public List<Connection> connections = new List<Connection>();
        public int activeConnection = 0;
        public int segmentInitialSplit = 128;
        public float segmentMergeAngleThreshold = 10f;

        private Vector3[] controlPoints;
        private SplineSegment[][] segmentList;

        public Vector3 GetClosestPoint(int segmentIndex, Vector3 point, out float t, out bool isBeforeStart, out bool isAfterEnd)
        {
            var segments = GetSegmentList(segmentIndex);

            var bestPosition = Vector3.zero;
            var bestDistance = float.MaxValue;
            var bestSegmentIndex = 0;
            var bestDot = 0f;
            
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var a = segments[i];
                var b = segments[i + 1];

                var ray = new Ray(a.point, b.point - a.point);
                var dot = Vector3.Dot(point - ray.origin, ray.direction);
                var distance = (b.point - a.point).magnitude;
                
                var pointOnRay = ray.GetPoint(Mathf.Clamp(dot, 0f, distance));

                t = Mathf.Lerp(a.t, b.t, dot / (b.point - a.point).magnitude);
                
                var score = 1f / (pointOnRay - point).sqrMagnitude;
                if (score > bestScore)
                {
                    bestPosition = pointOnRay;
                    bestScore = score;
                    bestSegmentIndex = i;
                    bestDot = dot;
                }
            }

            isBeforeStart = bestSegmentIndex == 0 && bestDot < 0f;
            isAfterEnd = bestSegmentIndex == segments.Length - 1 && bestDot > ;

            return bestPosition;
        }
        
        public Vector3 GetControlPoint(int connectionIndex, int i)
        {
            var connection = connections[connectionIndex];

            var previous = connection.previous != null ? connection.previous.transform.position : transform.position;
            var next = connection.next != null ? connection.next.transform.position : transform.position;

            return i switch
            {
                0 => (previous + transform.position) / 2f,
                1 => transform.position,
                2 => (next + transform.position) / 2f,
            };
        }

        public SplineSegment[] GetSegmentList(int i)
        {
            if (segmentList == null) UpdateSegmentList();
            return segmentList[i];
        }

        [ContextMenu("Update Segment List")]
        public void UpdateSegmentList()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            segmentList = new SplineSegment[connections.Count][];

            for (var i = 0; i < connections.Count; i++)
            {
                var list = new List<SplineSegment>(segmentInitialSplit);
                for (var j = 0; j < segmentInitialSplit; j++)
                {
                    var t = j / (segmentInitialSplit - 1f);
                    list.Add(new SplineSegment(Sample(i, t), t));
                }

                while (true)
                {
                    if (list.Count < 3) break;
                    
                    var smallestAngle = float.MaxValue;
                    var smallestAngleIndex = 0;
                    
                    for (var j = 0; j < list.Count - 2; j++)
                    {
                        var a = list[j];
                        var b = list[j + 1];
                        var c = list[j + 2];

                        var ab = (b.point - a.point);
                        var bc = (c.point - b.point);

                        var angle = Vector3.Angle(ab, bc);
                        if (angle < smallestAngle)
                        {
                            smallestAngle = angle;
                            smallestAngleIndex = j;
                        }
                    }

                    if (smallestAngle > segmentMergeAngleThreshold) break;

                    list.RemoveAt(smallestAngleIndex + 1);
                }
                
                segmentList[i] = list.ToArray();
            }
        }

        public Vector3 Sample(int index, float t)
        {
            var a = GetControlPoint(index, 0);
            var b = GetControlPoint(index, 1);
            var c = GetControlPoint(index, 2);

            return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
        }

        private void OnValidate()
        {
            activeConnection = (activeConnection % connections.Count + connections.Count) % connections.Count;

            for (var i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection.previous != null)
                {
                    if (!connection.previous.connections.Exists(e => e.next == this))
                    {
                        connection.previous.connections.Add(new Connection(null, this));
                    }
                }

                if (connection.next != null)
                {
                    if (!connection.next.connections.Exists(e => e.previous == this))
                    {
                        connection.next.connections.Add(new Connection(this, null));
                    }
                }
            }
        }

        [System.Serializable]
        public struct Connection
        {
            public TrackSegment previous;
            public TrackSegment next;

            public Connection(TrackSegment previous, TrackSegment next)
            {
                this.previous = previous;
                this.next = next;
            }
        }

        public struct SplineSegment
        {
            public Vector3 point;
            public float t;

            public SplineSegment(Vector3 point, float t)
            {
                this.point = point;
                this.t = t;
            }
        }
    }
}