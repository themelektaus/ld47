using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	public static class ExtensionMethods
	{
		public static Vector2 Rotate2D(this Vector2 @this, float angle) {
			angle *= Mathf.Deg2Rad;
			return new Vector2(
				@this.x * Mathf.Cos(angle) - @this.y * Mathf.Sin(angle),
				@this.x * Mathf.Sin(angle) + @this.y * Mathf.Cos(angle)
			);
		}

		public static bool SetSingleton<T>(this MonoBehaviour @this, ref T instance) where T : MonoBehaviour {
			if (instance) {
				Debug.LogWarning("Singleton already setted");
				return false;
			}
			instance = @this as T;
			Singleton<MonoBehaviour>.singletons.Add(@this);
			return true;
		}

		public static bool UnsetSingletone<T>(this MonoBehaviour @this, ref T instance) where T : MonoBehaviour {
			if (!instance) {
				Debug.LogWarning("Singleton not set");
				return false;
			}
			if (@this != instance) {
				Debug.LogWarning("Singleton is not this");
				return false;
			}
			var singletons = Singleton<MonoBehaviour>.singletons;
			if (!singletons.Contains(@this)) {
				Debug.LogWarning("Singleton is not listed");
				return false;
			}
			singletons.Remove(@this);
			instance = null;
			return true;
		}

		public static GameObject ToTempInstance(this GameObject @this) {
			Utility.ConvertToTempInstance(@this);
			return @this;
		}

		public static byte GetRingIndex(this Object @this) {
			return @this.GetFromCache<IObjectInfo>().GetRingIndex();
		}

		public static void SetRingIndex(this Object @this, byte ringIndex) {
			@this.GetFromCache<IObjectInfo>().SetRingIndex(ringIndex);
		}
	}
}