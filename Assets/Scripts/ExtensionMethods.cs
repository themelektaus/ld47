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
			Singleton<MonoBehaviour>.singletons.Add(@this);
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
			if (!Singleton<MonoBehaviour>.singletons.Contains(@this)) {
				Debug.LogWarning("Singleton is not listed");
				return;
			}
			Singleton<MonoBehaviour>.singletons.Remove(@this);
			instance = null;
		}

		public static GameObject ToTempInstance(this GameObject @this) {
			Utils.ConvertToTempInstance(@this);
			return @this;
		}

		// public static void Send(this TNBehaviour @this, string rfcName, int playerID) {
		// 	if (@this.tno && !@this.tno.hasBeenDestroyed) {
		// 		@this.tno.Send(rfcName, playerID);
		// 	}
		// }

		// public static void Send(this TNBehaviour @this, string rfcName, Target target) {
		// 	if (@this.tno && !@this.tno.hasBeenDestroyed) {
		// 		@this.tno.Send(rfcName, target);
		// 	}
		// }

		// public static void Send(this TNBehaviour @this, string rfcName, Target target, object obj0) {
		// 	if (@this.tno && !@this.tno.hasBeenDestroyed) {
		// 		@this.tno.Send(rfcName, target, obj0);
		// 	}
		// }

		// public static void Send(this TNBehaviour @this, string rfcName, Target target, object obj0, object obj1) {
		// 	if (@this.tno && !@this.tno.hasBeenDestroyed) {
		// 		@this.tno.Send(rfcName, target, obj0, obj1);
		// 	}
		// }
	}
}