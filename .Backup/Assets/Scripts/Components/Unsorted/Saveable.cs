using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class Saveable : MonoBehaviour
	{
		public bool logging;
		public bool autoLoad;
		public bool autoSave;
		public string playerPrefsKey;
		public FieldCollection fields = new FieldCollection();

		void Awake() {
			this.Logging(logging);
		}

		void OnEnable() {
			if (autoLoad) {
				Load();
			}
		}

		void OnDisable() {
			if (autoSave) {
				Save();
			}
		}

		public void Save() {
			this.Log("Save()");
			if (string.IsNullOrEmpty(playerPrefsKey)) {
				Debug.LogError("Save: playerPrefsKey is null");
				return;
			}
			PlayerPrefs.SetString(playerPrefsKey, fields.Save());
		}

		public void Load() {
			this.Log("Load()");
			if (string.IsNullOrEmpty(playerPrefsKey)) {
				Debug.LogError("Load: playerPrefsKey is null");
				return;
			}
			if (PlayerPrefs.HasKey(playerPrefsKey)) {
				fields.Load(PlayerPrefs.GetString(playerPrefsKey));
			} else {
				Debug.LogWarning($"Load: playerPrefsKey \"{playerPrefsKey}\" not exists");
			}
		}
	}
}