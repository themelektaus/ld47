using UnityEngine;
using UnityEngine.Serialization;

namespace MT.Packages.LD47
{
	public class AutoDestroy : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("after")] float after = 0;

		void Awake() {
			Update();
		}

		void Update() {
			if (after <= 0) {
				Destroy(gameObject);
			}
			after -= Time.deltaTime;
		}
	}
}