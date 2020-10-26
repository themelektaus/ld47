using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using System.IO;
#endif

namespace MT.Packages.LD47
{
	public static class Utils
	{
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
			return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg + 90;
		}

		public static bool TryGetMyID(out uint id) {
			id = 0;
			if (Mirror.NetworkServer.active) {
				return true;
			}
			if (Mirror.NetworkClient.active && Mirror.NetworkClient.connection.identity) {
				id = Mirror.NetworkClient.connection.identity.netId;
				return true;
			}
			return false;
		}

		public static bool IsMine(uint id) {
			return TryGetMyID(out var myID) && myID == id;
		}

		public static bool TryGetConnection(uint netId, out Mirror.NetworkConnectionToClient connection) {
			var connections = Mirror.NetworkServer.connections.Values;
			foreach (var _connection in connections.Where(x => x.identity && x.identity.netId == netId)) {
				connection = _connection;
				return true;
			}
			connection = null;
			return false;
		}

#if UNITY_EDITOR

		struct ResourcesFilesInfo
		{
			public List<string> filelist;
			public System.DateTime timestamp;
		}
		static readonly Dictionary<string, ResourcesFilesInfo> resourcesFilesInfos = new Dictionary<string, ResourcesFilesInfo>();

		public static List<string> GetFilesByFolder(string folderName, params string[] extensions) {
			var key = folderName + "|" + string.Join(" | ", extensions);
			if (resourcesFilesInfos.ContainsKey(key)) {
				if ((System.DateTime.Now - resourcesFilesInfos[key].timestamp).TotalSeconds < 5) {
					return resourcesFilesInfos[key].filelist.ToList();
				}
				var info = resourcesFilesInfos[key];
				info.filelist.Clear();
				info.timestamp = System.DateTime.Now;
				resourcesFilesInfos[key] = info;
			} else {
				resourcesFilesInfos[key] = new ResourcesFilesInfo {
					filelist = new List<string>(),
					timestamp = System.DateTime.Now
				};
			}
			var filelist = resourcesFilesInfos[key].filelist;
			foreach (var folder in GetFoldersByName(folderName)) {
				foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)) {
					string filename = file.Substring(folder.Length + 1);
					foreach (var extension in extensions) {
						var ext = "." + extension.ToLower();
						if (filename.ToLower().EndsWith(ext)) {
							filelist.Add(filename.Substring(0, filename.Length - ext.Length));
							break;
						}
					}
				}
			}
			return filelist.ToList();
		}

		static List<string> GetFoldersByName(string name) {
			List<string> result = new List<string>();
			Stack<string> stack = new Stack<string>();
			stack.Push(Application.dataPath);
			while (stack.Count > 0) {
				string path = stack.Pop();
				foreach (string folder in Directory.GetDirectories(path)) {
					if (Path.GetFileName(folder).Equals(name)) {
						result.Add(folder);
					}
					stack.Push(folder);
				}
			}
			return result;
		}
#endif
	}
}