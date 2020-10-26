using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool_Object_Explosion : DefaultPool_Object
	{
		public float damage = 3;

		readonly List<IHostile> hostiles = new List<IHostile>();

		Collider2D _collider;

		void Awake() {
			_collider = GetComponent<Collider2D>();
		}

		public override void OnEnableAll() {
			base.OnEnableAll();
			if (info.isMine) {
				_collider.enabled = true;
				StartCoroutine(DisableRoutine());
			}
		}

		IEnumerator DisableRoutine() {
			yield return new WaitForSeconds(.3f);
			_collider.enabled = false;
			hostiles.Clear();
		}

		protected override void OnTrigger(Collider2D collision) {
			base.OnTrigger(collision);
			if (collision.TryGetComponent<IHostile>(out var hostile)) {
				if (!hostiles.Contains(hostile)) {
					hostile.ReceiveDamage(info.ownerID, damage);
					hostiles.Add(hostile);
				}
			}
		}
	}
}