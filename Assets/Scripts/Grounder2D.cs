using System.Collections.Generic;
using UnityEngine;

public class Grounder2D : MonoBehaviour
{
	List<Collider2D> collisions = new List<Collider2D>();

	public bool grounded {
		get {
			return collisions.Count > 0;
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (collision.isTrigger) {
			return;
		}
		if (collision.TryGetComponent<Affector2D>(out _)) {
			return;
		}
		collisions.Add(collision);
	}

	void OnTriggerExit2D(Collider2D collision) {
		collisions.Remove(collision);
	}
}