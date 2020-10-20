using System.Collections;
using UnityEngine;
using G = System.Collections.Generic;

namespace MT.Packages.LD47
{
    public class Explosion : MonoBehaviour
    {
		[ReadOnly] public int playerID;

		Collider2D _collider;
        G.List<IHostile> hostiles = new G.List<IHostile>();

		void Awake() {
			_collider = GetComponent<Collider2D>();
			// gameObject.DestroySelf(2);
		}

		IEnumerator Start() {
			yield return new WaitForSeconds(.3f);
			_collider.enabled = false;
		}

		void OnTriggerStay2D(Collider2D collision) {
			if (/* playerID != TNManager.playerID || */ collision.isTrigger) {
				return;
			}
			if (collision.TryGetComponent<IHostile>(out var hostile)) {
				if (!hostiles.Contains(hostile)) {
					hostile.ReceiveDamage(tag, 2);
					hostiles.Add(hostile);
				}
			}
		}
	}
}