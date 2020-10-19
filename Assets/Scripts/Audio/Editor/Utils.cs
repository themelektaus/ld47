using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MT.Packages.LD47.Audio.Editor
{
	public static class EditorUtils
	{
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
	}
}