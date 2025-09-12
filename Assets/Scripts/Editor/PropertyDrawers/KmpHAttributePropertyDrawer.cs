using Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(KmpHAttribute))]
    public class KmpHAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                base.OnGUI(position, property, label);
                return;
            }

            var suffixSize = 36f;
            property.floatValue = EditorGUI.FloatField(new Rect(position.x, position.y, position.width - suffixSize - 2, position.height), label, property.floatValue * 3.6f) / 3.6f;
            EditorGUI.LabelField(new Rect(position.x + position.width - suffixSize, position.y, suffixSize, position.height), "Km/h");
        }
    }
}