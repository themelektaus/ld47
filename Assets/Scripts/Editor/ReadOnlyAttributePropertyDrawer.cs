using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47.Editor
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyAttributePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			GUI.enabled = (attribute as ReadOnlyAttribute).duringPlayMode && !Application.isPlaying;
			EditorGUI.PropertyField(position, property, true);
			GUI.enabled = true;

		}
	}
}