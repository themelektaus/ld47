using UnityEngine;
using UnityEditor;

namespace MT.Packages.LD47.Audio
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.Vector2) {
                return;
            }
            var attr = attribute as MinMaxAttribute;

			var p = new Rect(position.x, position.y, position.width - 84, EditorGUIUtility.singleLineHeight);

			Vector2 value = property.vector2Value;
            EditorGUI.MinMaxSlider(p, label, ref value.x, ref value.y, attr.Min, attr.Max);

            p.x += p.width;
            p.width = 38;
            value.x = EditorGUI.FloatField(p, value.x);

            p.x += 40;
            value.y = EditorGUI.FloatField(p, value.y);

            property.vector2Value = value;
        }
    }
}