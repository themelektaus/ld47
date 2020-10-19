using UnityEngine;

namespace MT.Packages.LD47
{
	public class AutoDestroy : MonoBehaviour
	{
		public float after;

		void Update() {
			after -= Time.deltaTime;
			if (after <= 0) {
				Destroy(gameObject);
			}
		}
	}
}