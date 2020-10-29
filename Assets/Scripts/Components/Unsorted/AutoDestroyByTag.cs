using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class AutoDestroyByTag : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] string _tag = "Auto Destroy";

		void Awake() {
			foreach (var gameObject in from t in Resources.FindObjectsOfTypeAll<Transform>()
									   where t.hideFlags == HideFlags.None && t.CompareTag(_tag)
									   where t.gameObject.scene == gameObject.scene
									   select t.gameObject
			) {
				Destroy(gameObject);
			}
		}
#endif
	}
}