using MT.Packages.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class Grounder : MonoBehaviour
	{
		readonly List<Collider2D> collisions = new List<Collider2D>();

		public bool grounded {
			get {
				return collisions.Count > 0;
			}
		}

		void OnTriggerEnter2D(Collider2D collision) {
			if (collision.isTrigger) {
				return;
			}
			if (collision.HasComponent<Attractor>()) {
				if (!collision.HasComponent<NetworkRigidbody2D>()) {
					return;
				}
			}
			collisions.Add(collision);
		}

		void OnTriggerExit2D(Collider2D collision) {
			collisions.Remove(collision);
		}
	}
}