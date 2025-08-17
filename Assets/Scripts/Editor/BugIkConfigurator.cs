using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Editor
{
    public class BugIkConfigurator : EditorWindow
    {
        [MenuItem("Tools/IK/Bug IK Configurator")]
        public static void OpenWindow() => CreateWindow<BugIkConfigurator>("Bug IK Configurator");

        public Transform root;
        public Rig rig;
        
        private void OnGUI()
        {
            root = EditorGUILayout.ObjectField("Root", root, typeof(Transform), true) as Transform;
            rig = EditorGUILayout.ObjectField("Root", rig, typeof(Rig), true) as Rig;

            if (GUILayout.Button("Do thing"))
            {
                var regex = new Regex(@"Upper Leg\.\d\.[LR]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (regex.IsMatch(child.name))
                    {
                        var leg = new Leg(child);
                        var ikComponent = new GameObject($"IK Rig.{leg.index}.{leg.side}").AddComponent<TwoBoneIKConstraint>();
                        ikComponent.transform.SetParent(rig.transform);
                        ikComponent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        
                        var ikData = ikComponent.data;

                        ikData.root = leg.root;
                        ikData.mid = leg.mid;
                        ikData.tip = leg.tip;
                        ikData.hint = leg.hint;
                        ikData.target = leg.target;
                        
                        ikComponent.data = ikData;
                    }
                }
            }
        }

        public struct Leg
        {
            public Transform root;
            public Transform mid;
            public Transform tip;
            public Transform hint;
            public Transform target;
            
            public int index;
            public char side;

            public Leg(Transform root)
            {
                this.root = root;
                mid = root.GetChild(0);
                tip = mid.GetChild(0);

                index = root.name[^3] - '0';
                side = root.name[^1];

                hint = root.parent.Find($"Leg IK Hint.{index}.{side}");
                target = root.parent.Find($"Leg IK Target.{index}.{side}");
            }
        }
    }
}