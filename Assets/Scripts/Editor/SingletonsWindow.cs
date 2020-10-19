using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47.Editor
{
	public class SingletonsWindow : EditorWindow
	{
		[MenuItem("Window/Singletons")]
		static void Init() {
			GetWindow<SingletonsWindow>("Singletons");
		}

		Vector2 _scrollPosition;

		void OnGUI() {
			EditorGUILayout.BeginVertical(new GUIStyle {
				padding = new RectOffset(10, 10, 10, 5)
			}, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			{
				EditorGUILayout.LabelField("Singletons", EditorStyles.boldLabel);
				_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, new GUIStyle {
					padding = new RectOffset(0, 5, 0, 0)
				}, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{
					EditorGUI.BeginDisabledGroup(true);
					if (Singleton<MonoBehaviour>.singletons == null) {
						EditorGUILayout.LabelField("null");
					} else if (Singleton<MonoBehaviour>.singletons.Count == 0) {
						EditorGUILayout.LabelField("Count: 0");
					} else {
						foreach (var singleton in Singleton<MonoBehaviour>.singletons) {
							EditorGUILayout.ObjectField(singleton, typeof(MonoBehaviour), false);
						}
						EditorGUI.EndDisabledGroup();
					}
				}
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}
	}
}