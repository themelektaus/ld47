using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MT.Packages.LD47.Audio
{
	public static class Utils
	{
#if UNITY_EDITOR
		public static Object FindAndLoadAsset(string filter, System.Type type) {
			foreach (var guid in FindAssets(filter)) {
				return LoadAsset(guid, type);
			}
			return null;
		}

		public static T FindAndLoadAsset<T>(string filter) where T : Object {
			return FindAndLoadAssets<T>(filter).FirstOrDefault();
		}

		public static IEnumerable<T> FindAndLoadAssets<T>(string filter) where T : Object {
			foreach (var guid in FindAssets(filter)) {
				yield return LoadAsset<T>(guid);
			}
		}

		public static string[] FindAssets(string filter) {
			return AssetDatabase.FindAssets(filter);
		}

		public static Object LoadAsset(string guid, System.Type type) {
			return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
		}

		public static T LoadAsset<T>(string guid) where T : Object {
			return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		}

		public static void UpdateSoundEffects(bool ignoreErrors) {
			var audioLibrary = FindAndLoadAsset<AudioLibrary>("t:AudioLibrary");
			if (audioLibrary) {
				foreach (var soundEffect in FindAndLoadAssets<SoundEffect>("t:SoundEffect")) {
					soundEffect.audioLibrary = audioLibrary;
				}
				if (audioLibrary.logging) {
					Debug.Log("Sound Effects updated :)");
				}
			} else if (!ignoreErrors) {
				Debug.LogError("No Audio Library found :(");
			}
		}
#endif
	}
}