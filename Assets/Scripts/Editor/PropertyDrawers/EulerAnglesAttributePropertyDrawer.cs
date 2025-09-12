using Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EulerAnglesAttribute))]
    public class EulerAnglesAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.quaternionValue = Quaternion.Euler(EditorGUI.Vector3Field(position, label, property.quaternionValue.eulerAngles));
        }
    }
}