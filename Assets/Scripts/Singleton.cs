using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
		public static List<MonoBehaviour> singletons = new List<MonoBehaviour>();

        static T _instance;

		public static T instance {
			get {
				if (!_instance) {
					_instance = Utils.CreateInstance<T>();
				}
				return _instance;
			}
		}

		void Awake() {
			this.SetSingleton(ref _instance);
			OnAwake();
		}

		protected virtual void OnAwake() {

		}

		void OnDestroy() {
			this.UnsetSingletone(ref _instance);
		}
	}
}