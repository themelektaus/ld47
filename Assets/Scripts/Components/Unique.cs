using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public abstract class Unique : MonoBehaviour
	{
		[ReadOnly] public string ID;

		string _ID;

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
			if (string.IsNullOrEmpty(ID) || !ID.StartsWith("[" + gameObject.scene.name + "] ")) {
				var IDs = FindObjectsOfType<Unique>().Select(x => x.ID).ToArray();
				do {
					ID = "[" + gameObject.scene.name + "] " + Guid.NewGuid();
				} while (IDs.Any(id => id == ID));
#if UNITY_EDITOR
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
			}
		}
	}
}