using System.Drawing.Drawing2D;
using PlasticPipe.PlasticProtocol.Messages;
using Runtime.Train;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
    [CustomEditor(typeof(Locomotive))]
    public class LocomotiveInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var rect = EditorGUILayout.GetControlRect(false, 20 * 6f);
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));

            var locomotive = target as Locomotive;
            var step = 1f / 128f;
            for (var t = 0f; t < 1f; t += step)
            {
                var t0 = t;
                var t1 = Mathf.Min(1f, t + step);

                var v0 = locomotive.enginePowerCurve.Evaluate(t0);
                var v1 = locomotive.enginePowerCurve.Evaluate(t1);

                var p0 = rect.min + new Vector2(t0 * rect.width, (1f - v0) * rect.height);
                var p1 = rect.min + new Vector2(t1 * rect.width, (1f - v1) * rect.height);
                
                DrawLine(p0, p1, Color.red);
            }

            if (Application.isPlaying)
            {
                var speed = Mathf.Clamp01(Mathf.Abs(locomotive.engineVelocity) / locomotive.enginePowerMaxSpeed);
                var p0 = rect.min + new Vector2(speed * rect.width, 0f);
                var p1 = rect.min + new Vector2(speed * rect.width, rect.height);
                
                DrawLine(p0, p1, Color.green);
            }
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float width = 2f)
        {
            var matrix = GUI.matrix;

            var vector = end - start;
            var direction = vector.normalized;
            var rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GUI.matrix *= Matrix4x4.TRS(start, Quaternion.Euler(0f, 0f, rotation), Vector3.one);
            EditorGUI.DrawRect(new Rect(0f, 0f, vector.magnitude, width), color);
            
            GUI.matrix = matrix;
        }
    }
}