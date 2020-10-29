using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47.Editor
{
	[CustomPropertyDrawer(typeof(ResourcePathAttribute))]
	public class ResourcePathAttributePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var (files, index) = Begin(property.stringValue, (attribute as ResourcePathAttribute).extensions);
			index = EditorGUI.Popup(position, label.text, index, files);
			property.stringValue = End(files, index);
		}

		public static (string[], int) Begin(string value, params string[] extensions) {
			var files = Utils.GetFilesByFolder("Resources", extensions);
			files.Insert(0, "-");
			int index = files.IndexOf(value);
			if (index == -1) {
				index = 0;
			}
			return (files.ToArray(), index);
		}

		public static string End(string[] files, int index) {
			if (index == 0) {
				return "";
			}
			return files[index];
		}
	}
}