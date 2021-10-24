using UnityEngine;

namespace MT.Packages.LD47
{
	public class Spectator : Singleton<Spectator>
	{
		public float speed = 1;

		public void Activate(Vector3 position) {
			if (!enabled) {
				transform.position = position;
			}
			enabled = true;
			CameraControl.instance.SetTarget(transform, 5, .5f);
		}

		public void Deactivate(Transform alternativeTarget) {
			enabled = false;
			CameraControl.instance.SetTarget(alternativeTarget, 20, .05f);
		}

		void Update() {
			var position = transform.position;
			var f = Time.deltaTime * speed * Camera.main.orthographicSize;
			position.x += Input.GetAxis("Horizontal") * f;
			position.y += Input.GetAxis("Vertical") * f;
			transform.position = position;
		}
	}
}