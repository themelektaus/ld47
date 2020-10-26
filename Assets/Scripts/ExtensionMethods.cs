using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public static class ExtensionMethods
	{
		public static void SetSingleton<T>(this MonoBehaviour @this, ref T instance) where T : MonoBehaviour {
			if (instance) {
				Debug.LogWarning("Singleton already setted");
				return;
			}
			instance = @this as T;
			Singleton<MonoBehaviour>.GetSingletons().Add(@this);
		}

		public static void UnsetSingletone<T>(this MonoBehaviour @this, ref T instance) where T : MonoBehaviour {
			if (!instance) {
				Debug.LogWarning("Singleton not set");
				return;
			}
			if (@this != instance) {
				Debug.LogWarning("Singleton is not this");
				return;
			}
			var singletons = Singleton<MonoBehaviour>.GetSingletons();
			if (!singletons.Contains(@this)) {
				Debug.LogWarning("Singleton is not listed");
				return;
			}
			singletons.Remove(@this);
			instance = null;
		}

		public static GameObject ToTempInstance(this GameObject @this) {
			Utils.ConvertToTempInstance(@this);
			return @this;
		}

		public static bool HasComponent<T>(this Component @this) {
			return @this.TryGetComponent<T>(out _);
		}

		public static bool HasAnyComponent<T1, T2>(this Component @this) {
			return @this.HasComponent<T1>() || @this.HasComponent<T2>();
		}

		public static bool HasAnyComponent<T1, T2, T3>(this Component @this) {
			return @this.HasAnyComponent<T1, T2>() || @this.HasComponent<T3>();
		}

		public static bool HasAnyComponent<T1, T2, T3, T4>(this Component @this) {
			return @this.HasAnyComponent<T1, T2, T3>() || @this.HasComponent<T4>();
		}

		static readonly Dictionary<object, bool> logHandles = new Dictionary<object, bool>();

		static void Log(object handle, System.Action callback) {
			if (!logHandles.ContainsKey(handle)) {
				logHandles.Add(handle, true);
			}
			if (logHandles[handle]) {
				callback();
			}
		}

		public static void Logging(this object handle, bool enabled) {
			logHandles[handle] = enabled;
		}

		public static void Log(this object handle, object message) {
			Log(handle, () => Debug.Log(message));
		}

		public static void LogWarning(this object handle, object message) {
			Log(handle, () => Debug.LogWarning(message));
		}

		public static void LogError(this object handle, object message) {
			Log(handle, () => Debug.LogError(message));
		}

		// --- Not longer needed ---
		// public static bool IsMine(this Component @this) {
		// 	if (@this.TryGetComponent<Mirror.NetworkIdentity>(out var identity)) {
		// 		if (Utils.IsMine(identity.netId)) {
		// 			return true;
		// 		}
		// 	}
		// 	return false;
		// }
	}
}