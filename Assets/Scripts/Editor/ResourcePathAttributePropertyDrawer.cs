using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47
{
	[CustomPropertyDrawer(typeof(ResourcePathAttribute))]
	public class ResourcePathAttributePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var files = Utils.GetResourcesFiles((attribute as ResourcePathAttribute).Extensions);
			files.Insert(0, "-");
			int index = files.IndexOf(property.stringValue);
			if (index == -1) {
				index = 0;
			}
			index = EditorGUI.Popup(position, label.text, index, files.ToArray());
			if (index == 0) {
				property.stringValue = "";
			} else {
				property.stringValue = files[index];
			}
		}
	}
}