using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#pragma warning disable IDE0051

namespace MT.Packages.LD47.Editor
{
	using ResourcePathProperty = Core.Editor.ResourcePathAttributePropertyDrawer;

	public class LD47Tools : EditorWindow
	{
		[MenuItem("Window/LD47 Tools")]
		private static void Init() {
			GetWindow<LD47Tools>("LD47 Tools");
		}

		static GUIStyle Padding(int left, int right, int top, int bottom) {
			return new GUIStyle { padding = new RectOffset(left, right, top, bottom) };
		}

		static GUILayoutOption[] Expanded {
			get { return new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) }; }
		}

		class SceneObjectPopup<T> where T : Object
		{
			T[] objects = new T[0];
			int index = -1;

			public bool Draw(string label, out T value) {
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				EditorGUILayout.BeginHorizontal();
				{
					objects = objects.Where(x => x).ToArray();
					if (objects.Length == 0) {
						index = -1;
					}
					if (index == -1) {
						EditorGUI.BeginDisabledGroup(true);
						GUILayout.Button("◀");
						EditorGUILayout.Popup(0, new[] { "(None)" });
						GUILayout.Button("▶");
						EditorGUI.EndDisabledGroup();
						if (GUILayout.Button("Find", GUILayout.Width(70))) {
							objects = FindObjectsOfType<T>();
							index = objects.Length > 0 ? 0 : -1;
						}
					} else {
						EditorGUI.BeginDisabledGroup(index == 0);
						if (GUILayout.Button("◀")) {
							index--;
						}
						EditorGUI.EndDisabledGroup();
						index = Mathf.Clamp(EditorGUILayout.Popup(index, objects.Select(x => x.name).ToArray()), 0, objects.Length - 1);
						EditorGUI.BeginDisabledGroup(index == objects.Length - 1);
						if (GUILayout.Button("▶")) {
							index++;
						}
						EditorGUI.EndDisabledGroup();
						if (GUILayout.Button("Clear", GUILayout.Width(70))) {
							index = -1;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				return TryGetValue(out value);
			}

			public bool TryGetValue(out T value) {
				if (index > -1) {
					value = objects[index];
					return true;
				}
				value = null;
				return false;
			}
		}

		[SerializeField] List<Vector2> scrollPositions;
		[SerializeField] int scrollPositionIndex;

		[SerializeField] Dictionary<Object, bool> foldouts;

		[SerializeField] int index;

		SceneObjectPopup<Character> characters = new SceneObjectPopup<Character>();

		SceneObjectPopup<Gravitation> gravitations = new SceneObjectPopup<Gravitation>();
		Transform[] gravitationTransforms;
		float gravitationVelocity;

		void OnGUI() {
			if (scrollPositions == null) {
				scrollPositions = new List<Vector2>();
			}
			scrollPositionIndex = 0;
			if (foldouts == null) {
				foldouts = new Dictionary<Object, bool>();
			}
			EditorGUILayout.BeginVertical(Padding(10, 10, 10, 5), Expanded);
			{
				index = GUILayout.Toolbar(index, new[] { "Runtime", "Character", "Gravitation", "Cache / GC" }, GUILayout.Height(25));
				GUILayout.Space(10);
				switch (index) {
					case 0: OnGUI_0(); break;
					case 1: OnGUI_1(); break;
					case 2: OnGUI_2(); break;
					case 3: OnGUI_3(); break;
				}
			}
			EditorGUILayout.EndVertical();
		}

		void OnGUI_0() {
			EditorGUILayout.LabelField("Singletons", EditorStyles.boldLabel);
			BeginScrollView();
			{
				EditorGUI.BeginDisabledGroup(true);
				{
					var singletons = Singleton<MonoBehaviour>.singletons;
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
			EndScrollView();

			EditorGUILayout.LabelField("Pools", EditorStyles.boldLabel);
			BeginScrollView();
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
			EndScrollView();

			EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
			BeginScrollView();
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
			EndScrollView();
		}

		void OnGUI_1() {
			BeginScrollView();

			if (characters.Draw("Character", out var character)) {

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

				if (character.TryGetComponent(out CharacterBot bot)) {
					GUILayout.Space(5);
					if (Foldout(bot, "Bot")) {
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
			EndScrollView();
		}

		void OnGUI_2() {
			BeginScrollView();
			{
				if (gravitations.Draw("Gravitation", out var gravitation)) {
					if (Event.current.modifiers == EventModifiers.Shift && gravitation && Selection.activeTransform) {
						gravitationTransforms = Selection.transforms;
					} else {
						gravitationTransforms = null;
					}
				} else {
					gravitationTransforms = null;
				}
			}
			EndScrollView();
		}

		void OnGUI_3() {
			OnGUI_3_Part("Components", position.height / 2 - 60, Core.Cache.cacheComponents);
			OnGUI_3_Part("Components (Parent)", position.height / 2 - 100, Core.Cache.cacheComponentsInParent);
		}

		void OnGUI_3_Part(string label, float height, Core.CacheObject cacheObject) {
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			BeginScrollView(height);
			{
				if (cacheObject != null) {
					var gameObjects = cacheObject.gameObjects;
					foreach (var key in gameObjects.Keys) {
						if (!Foldout(key, key ? key.name : "\"null\"")) {
							continue;
						}
						EditorGUILayout.ObjectField(key, typeof(GameObject), true);
						var gameObject = gameObjects[key];
						foreach (var type in gameObject.Keys) {
							OnGUI_3_Value(type.Name, gameObject[type]);
						}
						EditorGUILayout.Space(5);
					}
				}
			}
			EndScrollView();
			EditorGUI.BeginDisabledGroup(cacheObject == null);
			var buttonText = "Update Garbage Collector";
			if (cacheObject != null) {
				var lastGCUpdate = System.Math.Round((System.DateTime.Now - cacheObject.lastGCUpdate).TotalSeconds).ToString();
				buttonText += " (Last Time: " + lastGCUpdate + "s ago)";
			}
			if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) {
				padding = new RectOffset(5, 5, 5, 5)
			})) {
				cacheObject.UpdateGarbageCollector(true);
			}
			EditorGUILayout.Space(5);
			EditorGUI.EndDisabledGroup();
		}

		void OnGUI_3_Value(string label, object value) {
			if (value is bool boolValue) {
				EditorGUILayout.Toggle(label, boolValue);
				return;
			}

			if (value is int intValue) {
				EditorGUILayout.IntField(label, intValue);
				return;
			}

			if (value is float floatValue) {
				EditorGUILayout.FloatField(label, floatValue);
				return;
			}

			if (value is Object @object) {
				EditorGUILayout.ObjectField(label, @object, typeof(Object), true);
				if (value is IObjectInfo objectInfo) {
					EditorGUILayout.IntField("Ring Index", objectInfo.GetRingIndex());
				}
				return;
			}

			BeginScrollView(54);
			{
				var width = (position.width - 60) / 4;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(label, GUILayout.Width(width));
				EditorGUILayout.TextArea(value != null ? value.ToString() : "null", new GUIStyle(GUI.skin.textArea) {
					wordWrap = true
				}, GUILayout.Width(width * 2.9f), GUILayout.Height(54 - 6));
				EditorGUILayout.EndHorizontal();
			}
			EndScrollView();
		}

		void OnInspectorUpdate() {
			Repaint();
		}

		void Update() {
			if (gravitations.TryGetValue(out var gravitation) && gravitationTransforms != null) {
				foreach (var gravitationTransform in gravitationTransforms) {
					var eulerAngles = gravitationTransform.eulerAngles;
					var z = Utility.GetAngle2D(gravitationTransform.position, gravitation.transform.position);
					eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, z, ref gravitationVelocity, .05f);
					gravitationTransform.eulerAngles = eulerAngles;
				}
			}
		}

		void BeginScrollView(float height = 0) {
			while (scrollPositions.Count <= scrollPositionIndex) {
				scrollPositions.Add(new Vector2());
			}
			GUILayoutOption[] options = height == 0 ? Expanded : new[] { GUILayout.Height(height) };
			scrollPositions[scrollPositionIndex] = EditorGUILayout.BeginScrollView(
				scrollPositions[scrollPositionIndex], Padding(0, 5, 0, 0), options
			);
			scrollPositionIndex++;
		}

		void EndScrollView() {
			EditorGUILayout.EndScrollView();
		}

		bool Foldout(Object referenceObject, string content) {
			if (!foldouts.ContainsKey(referenceObject)) {
				foldouts.Add(referenceObject, false);
			}
			var foldout = foldouts[referenceObject];
			var color = GUI.color;
			GUI.color = foldout ? Color.white : new Color(1, 1, 1, .5f);
			if (GUILayout.Button((foldout ? "▼" : "▶") + " " + content, new GUIStyle(GUI.skin.button) {
				alignment = TextAnchor.MiddleLeft
			})) {
				foldout = !foldout;
			}
			GUI.color = color;
			foldouts[referenceObject] = foldout;
			return foldout;
		}
	}
}