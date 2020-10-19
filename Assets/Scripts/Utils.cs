using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace MT.Packages.LD47
{
	public static class Utils
	{
		public static T CreateInstance<T>() where T : MonoBehaviour {
			var gameObject = new GameObject();
			var result = gameObject.AddComponent<T>();
			gameObject.name = result.ToString();
			return result;
		}

		public static void ConvertToTempInstance(GameObject gameObject) {
			gameObject.transform.parent = TempInstances.instance.transform;
		}

		public static Vector2 GetDirection2D(Vector2 a, Vector2 b) {
			return (b - a).normalized;
		}

		public static float GetAngle2D(Vector2 direction) {
			return GetAngle2D(Vector2.zero, direction);
		}

		public static float GetAngle2D(Vector2 a, Vector2 b) {
			var p = a - b;
			return Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg + 90;
		}

		public static GameObject Instantiate(int channelID, string localPrefabName, System.Func<GameObject, GameObject> localFunc) {
			if (channelID == 0) {
				return null;
			}
			return localFunc(Resources.Load<GameObject>(localPrefabName));
		}

#if UNITY_EDITOR
		public static List<string> GetResourcesFiles(params string[] extensions) {
			List<string> result = new List<string>();
			foreach (var folder in GetResourcesFolders()) {
				foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)) {
					string filename = file.Substring(folder.Length + 1);
					foreach (var extension in extensions) {
						var ext = "." + extension.ToLower();
						if (filename.ToLower().EndsWith(ext)) {
							result.Add(filename.Substring(0, filename.Length - ext.Length));
							break;
						}
					}
				}
			}
			return result;
		}

		static List<string> GetResourcesFolders() {
			List<string> result = new List<string>();
			Stack<string> stack = new Stack<string>();
			stack.Push(Application.dataPath);
			while (stack.Count > 0) {
				string currentDir = stack.Pop();
				foreach (string dir in Directory.GetDirectories(currentDir)) {
					if (Path.GetFileName(dir).Equals("Resources")) {
						result.Add(dir);
					}
					stack.Push(dir);
				}
			}
			return result;
		}
#endif
	}
}