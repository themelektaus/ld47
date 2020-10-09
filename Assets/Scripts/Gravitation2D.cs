using UnityEngine;

public class Gravitation2D : MonoBehaviour
{
	public float gravityFactor = -2;

    Attractor2D[] affectors;

	public Vector2 position {
		get {
			var p = transform.position;
			return new Vector2(p.x, p.y);
		}
	}

	void Awake() {
		affectors = FindObjectsOfType<Attractor2D>();
		foreach (var affector in affectors) {
			affector.Register(this);
		}
	}
}