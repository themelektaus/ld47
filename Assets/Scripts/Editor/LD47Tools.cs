using System.Linq;
using UnityEditor;
using UnityEngine;

#pragma warning disable IDE0051

namespace MT.Packages.LD47.Editor
{
	using ResourcePathProperty = ResourcePathAttributePropertyDrawer;

	public class LD47Tools : EditorWindow
	{
		[MenuItem("Window/LD47 Tools")]
		private static void Init() {
			GetWindow<LD47Tools>("LD47 Tools");
		}

		Vector2[] _scrollPositions = new Vector2[6];

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
		[SerializeField] bool botFoldout;

		void OnGUI() {
			EditorGUILayout.BeginVertical(Padding(10, 10, 10, 5), Expanded);
			{
				index = GUILayout.Toolbar(index, new[] { "Runtime Objects", "Character", "Gravitation" }, GUILayout.Height(25));
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

			// if (!Mirror.NetworkServer.active) {
			// 	return;
			// }
			EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
			_scrollPositions[3] = EditorGUILayout.BeginScrollView(_scrollPositions[3], Padding(0, 5, 0, 0), Expanded);
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var characters = NetworkManager.entities.Where(x => x is Character).Select(x => x as Character).ToArray();
					if (characters.Length == 0) {
						EditorGUILayout.LabelField("Length: 0");
					} else {
						foreach (var character in characters) {
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("ID: " + (character ? character.netId.ToString() : "(NULL)"), GUILayout.Width(60));
							EditorGUILayout.ObjectField(character, typeof(Character), false);
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();
		}

		Character character;

		void OnGUI_1() {
			_scrollPositions[4] = EditorGUILayout.BeginScrollView(_scrollPositions[4], Padding(0, 5, 0, 0), Expanded);

			EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			{
				character = EditorGUILayout.ObjectField(character, typeof(Character), true) as Character;
				if (character) {
					if (GUILayout.Button("Clear", GUILayout.Width(70))) {
						character = null;
					}
				} else if (GUILayout.Button("Find", GUILayout.Width(70))) {
					character = FindObjectOfType<Character>();
					if (character) {
						Selection.activeGameObject = character.gameObject;
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if (character) {
				// PlayerController.instance.mode = (PlayerController.Mode) EditorGUILayout.EnumPopup("Mode", PlayerController.instance.mode);

				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle("Is Bot", character.isBot);
				EditorGUILayout.IntField("Ring Index", character.GetRingIndex());
				EditorGUILayout.ObjectField("Weapon", character.weaponInstance, typeof(Weapon), false);
				EditorGUI.EndDisabledGroup();

				var (files, index) = ResourcePathProperty.Begin(character.weaponName, "prefab");
				var newIndex = EditorGUILayout.Popup("Weapon", index, files);
				if (newIndex != index) {
					character.Remote_SetWeapon(ResourcePathProperty.End(files, newIndex));
				}

				if (character.hasAuthority && character.weaponInstance) {
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.FloatField("Current Ammo", character.weaponInstance.currentAmmo);
					EditorGUI.EndDisabledGroup();
				}

				character.TryGetComponent(out CharacterBot bot);
				if (bot) {
					botFoldout = EditorGUILayout.Foldout(botFoldout, "Bot");
					if (botFoldout) {
						EditorGUI.BeginDisabledGroup(true);
						{
							var botTarget = bot.target;

							EditorGUILayout.LabelField("Bot Target", EditorStyles.boldLabel);
							EditorGUILayout.ObjectField("Enemy", botTarget.enemy, typeof(Enemy), false);
							EditorGUILayout.ObjectField("Waypoint", botTarget.waypoint, typeof(Waypoint), false);
							EditorGUILayout.ObjectField("Character", botTarget.character, typeof(Character), false);
							EditorGUILayout.Toggle("Is Valid", botTarget.isValid != null && botTarget.isValid(botTarget.GetTransform()));

							EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
							EditorGUILayout.FloatField("Horizontal", bot.horizontal);
							EditorGUILayout.FloatField("Jump Time", bot.jumpTime);
							EditorGUILayout.FloatField("Horizontal Timer (Time)", bot.horizontalTimer.time);
							EditorGUILayout.FloatField("Target Timer (Time)", bot.targetTimer.time);
							EditorGUILayout.FloatField("Jump Timer (Time)", bot.jumpTimer.time);
						}
						EditorGUI.EndDisabledGroup();
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}

		Gravitation gravitation;
		Transform gravitationTransform;
		float gravitationVelocity;

		void OnGUI_2() {
			_scrollPositions[5] = EditorGUILayout.BeginScrollView(_scrollPositions[5], Padding(0, 5, 0, 0), Expanded);
			EditorGUILayout.LabelField("Gravitation", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			gravitation = EditorGUILayout.ObjectField(gravitation, typeof(Gravitation), true) as Gravitation;
			if (gravitation) {
				if (GUILayout.Button("Clear", GUILayout.Width(70))) {
					gravitation = null;
				}
			} else if (GUILayout.Button("Find", GUILayout.Width(70))) {
				gravitation = FindObjectOfType<Gravitation>();
			}
			EditorGUILayout.EndHorizontal();
			if (Event.current.modifiers == EventModifiers.Shift && gravitation && Selection.activeTransform) {
				gravitationTransform = Selection.activeTransform;
			} else {
				gravitationTransform = null;
			}
			EditorGUILayout.EndScrollView();
		}

		void Update() {
			if (gravitation && gravitationTransform) {
				var eulerAngles = gravitationTransform.eulerAngles;
				var z = Utils.GetAngle2D(gravitationTransform.position, gravitation.transform.position);
				eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, z, ref gravitationVelocity, .05f);
				gravitationTransform.eulerAngles = eulerAngles;
			}
		}
	}
}