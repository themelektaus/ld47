using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class AutoDestroyByTag : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] string _tag = "Auto Destroy";

		void Awake() {
			foreach (var gameObject in from t in Core.Utility.FindObjectsOfTypeAll<Transform>()
									   where t.gameObject.scene == gameObject.scene && t.CompareTag(_tag)
									   select t.gameObject
			) {
				Destroy(gameObject);
			}
		}
#endif
	}
}