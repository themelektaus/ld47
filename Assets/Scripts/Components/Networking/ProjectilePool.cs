using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool : NetworkPool
	{
		public GameObject Instantiate(string tag, Vector3 position, Vector3 targetPosition) {
			var direction = Utils.GetDirection2D(position, targetPosition);
			var projectile = Instantiate(position, direction);
			var gameObject = projectile.gameObject;
			gameObject.tag = tag;
			var transform = gameObject.transform;
			var a = transform.position;
			var b = position;
			a.x = b.x;
			a.y = b.y;
			transform.position = a;
			return gameObject;
		}
	}
}