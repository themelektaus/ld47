using UnityEngine;

namespace MT.Packages.LD47
{
	public class SpectatorCamera : MonoBehaviour
	{
		public float speed = 10;

		void Update() {
			var position = transform.position;
			var f = Time.deltaTime * speed;
			position.x += Input.GetAxis("Horizontal") * f;
			position.y += Input.GetAxis("Vertical") * f;
			transform.position = position;
		}
	}
}