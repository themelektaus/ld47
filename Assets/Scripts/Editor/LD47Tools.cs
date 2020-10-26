using UnityEditor;
using UnityEngine;

#pragma warning disable IDE0051

namespace MT.Packages.LD47.Editor
{
	public class LD47Tools : EditorWindow
	{
		[MenuItem("Window/LD47 Tools")]
		private static void Init() {
			GetWindow<LD47Tools>("LD47 Tools");
		}

		Vector2[] _scrollPositions = new Vector2[4];

		static GUIStyle Padding(int left, int right, int top, int bottom) {
			return new GUIStyle { padding = new RectOffset(left, right, top, bottom) };
		}

		static GUILayoutOption[] Expanded {
			get { return new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) }; }
		}

		void OnInspectorUpdate() {
			Repaint();
		}

		[SerializeField] int index;

		void OnGUI() {
			EditorGUILayout.BeginVertical(Padding(10, 10, 10, 5), Expanded);
			{
				index = GUILayout.Toolbar(index, new[] { "Runtime Objects", " ", " " }, GUILayout.Height(25));
				GUILayout.Space(10);
				switch (index) {
					case 0: OnGUI_0(); break;
					case 1: OnGUI_1(); break;
					case 2: OnGUI_2(); break;
				}
			}
			EditorGUILayout.EndVertical();
		}

		void OnGUI_0() {
			EditorGUILayout.LabelField("Singletons", EditorStyles.boldLabel);
			_scrollPositions[0] = EditorGUILayout.BeginScrollView(_scrollPositions[0], Padding(0, 5, 0, 0), Expanded);
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var singletons = Singleton<MonoBehaviour>.GetSingletons();
					if (singletons.Count == 0) {
						EditorGUILayout.LabelField("Count: 0");
					} else {
						foreach (var singleton in singletons) {
							EditorGUILayout.ObjectField(singleton, typeof(MonoBehaviour), false);
						}
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.LabelField("Pools", EditorStyles.boldLabel);
			_scrollPositions[1] = EditorGUILayout.BeginScrollView(_scrollPositions[1], Padding(0, 5, 0, 0), Expanded);
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var empty = true;
					foreach (var pool in Pool.GetAll()) {
						empty = false;
						EditorGUILayout.ObjectField(pool, typeof(Pool), false);
					}
					if (empty) {
						EditorGUILayout.LabelField("Count: 0");
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
			_scrollPositions[2] = EditorGUILayout.BeginScrollView(_scrollPositions[2], Padding(0, 5, 0, 0), Expanded);
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var buttons = Button.instances;
					if (buttons.Count == 0) {
						EditorGUILayout.LabelField("Count: 0");
					} else {
						foreach (var button in buttons) {
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.ObjectField(button, typeof(Button), false);
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();

			if (!Mirror.NetworkServer.active) {
				return;
			}
			EditorGUILayout.LabelField("Players", EditorStyles.boldLabel);
			_scrollPositions[3] = EditorGUILayout.BeginScrollView(_scrollPositions[3], Padding(0, 5, 0, 0), Expanded);
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var players = NetworkManager.players;
					if (players.Count == 0) {
						EditorGUILayout.LabelField("Count: 0");
					} else {
						foreach (var player in players) {
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("ID: " + (player ? player.netId.ToString() : "(NULL)"), GUILayout.Width(60));
							EditorGUILayout.ObjectField(player, typeof(Player), false);
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();
		}

		void OnGUI_1() {

		}

		void OnGUI_2() {

		}
	}
}