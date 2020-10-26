using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public abstract class Unique : MonoBehaviour
	{
		static List<Unique> uniques;

		public static List<Unique> GetUniques() {
			if (uniques == null) {
				uniques = new List<Unique>();
			} else {
				uniques.RemoveAll(x => !x);
			}
			return uniques;
		}

		static void Add(Unique unique) {
			var uniques = GetUniques();
			if (!uniques.Contains(unique)) {
				uniques.Add(unique);
			}
		}

		static void Remove(Unique unique) {
			var uniques = GetUniques();
			if (uniques.Contains(unique)) {
				uniques.Remove(unique);
			}
		}

		public static T Get<T>() where T : Unique {
			return GetUniques().Where(x => x is T).FirstOrDefault() as T;
		}

		public static T Get<T>(string id) where T : Unique {
			return Get(id) as T;
		}

		public static Unique Get(string id) {
			return GetUniques().Where(x => x.ID == id).FirstOrDefault();
		}

		[ReadOnly] public string ID;

		string _ID;

		protected virtual void Awake() {
			if (Application.isPlaying) {
				Add(this);
			}
		}

		protected virtual void OnDestroy() {
			if (Application.isPlaying) {
				Remove(this);
			}
		}

		void Start() {
#if UNITY_EDITOR
			foreach (var unique in FindObjectsOfType<Unique>()) {
				if (unique == this) {
					continue;
				}
				if (ID == unique.ID) {
					ID = "";
					break;
				}
			}
#endif
		}

		void Reset() {
			ID = _ID;
		}

		void OnValidate() {
			if (gameObject.scene.name == null) {
				ID = "";
			} else if (!string.IsNullOrEmpty(_ID)) {
#if UNITY_EDITOR
				if (PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.Connected) {
					ID = _ID;
				}
#endif
			}
		}

		void Update() {
			_ID = ID;
			if (Application.isPlaying) {
				return;
			}
#if UNITY_EDITOR
			if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) {
				return;
			}
#endif
			if (string.IsNullOrEmpty(ID)) {
				var IDs = FindObjectsOfType<Unique>().Select(x => x.ID).ToArray();
				do {
					ID = DateTime.Now.Ticks.ToString("x");
					System.Threading.Thread.Sleep(1);
				} while (IDs.Any(id => id == ID));
#if UNITY_EDITOR
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
			}
		}
	}
}