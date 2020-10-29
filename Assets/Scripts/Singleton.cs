using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		static List<MonoBehaviour> singletons;

		public static List<MonoBehaviour> GetSingletons() {
			if (singletons == null) {
				singletons = new List<MonoBehaviour>();
			} else {
				singletons.RemoveAll(x => !x);
			}
			return singletons;
		}

		static T _instance;

		public static bool exists { get { return _instance; } }

		public static T instance {
			get {
				if (!exists) {
					Debug.LogWarning($"Singleton<{typeof(T).Name}> not exists");
					return null;

				}
				return _instance;
			}
		}

		public static void CreateSingleton() {
			var gameObject = new GameObject();
			_instance = gameObject.AddComponent<T>();
			gameObject.name = _instance.ToString();
		}

		protected virtual void Awake() {
			this.SetSingleton(ref _instance);
		}

		protected virtual void OnDestroy() {
			this.UnsetSingletone(ref _instance);
		}
	}
}