using System;
using System.Reflection;
using Runtime.Train;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
    [CustomEditor(typeof(TrackSegment))]
    public class TrackSegmentEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var segment = target as TrackSegment;
            for (var i = 0; i < segment.connections.Count; i++)
            {
                Handles.color = Color.cyan;

                var segmentList = (Vector3[][])segment.GetType()
                    .GetField("segmentList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(segment);
                if (segmentList == null)
                {
                    var step = 1f / 16f;
                    for (var t = 0f; t < 1f; t += step)
                    {
                        var t0 = t;
                        var t1 = t + step;
                    
                        var p0 = segment.Sample(i, t0);
                        var p1 = segment.Sample(i, t1);
                    
                        Handles.DrawAAPolyLine(5f, p0, p1);
                    }
                }
                else
                {
                    Handles.DrawAAPolyLine(5f, segmentList[i]);
                }

                Handles.color = Color.green;
                Handles.DrawDottedLine(segment.GetControlPoint(i, 1), segment.GetControlPoint(i, 0), 5f);
                Handles.DrawDottedLine(segment.GetControlPoint(i, 1), segment.GetControlPoint(i, 2), 5f);
            }
        }
    }
}