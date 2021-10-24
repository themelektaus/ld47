using UnityEngine;

namespace MT.Packages.LD47
{
	public class Gravitation : MonoBehaviour
	{
		public float gravityFactor = -2;

		public Vector2 position {
			get {
				var p = transform.position;
				return new Vector2(p.x, p.y);
			}
		}
	}
}