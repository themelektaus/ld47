using MT.Packages.Core;
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

		protected override void Awake() {
			base.Awake();
			_collider = GetComponent<Collider2D>();
		}

		public override void OnEnableObject() {
			base.OnEnableObject();
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
			var hostile = collision.GetParentFromCache<IHostile>(true);
			if (hostile != null) {
				if (info.ownerRingIndex != (hostile as Component).GetRingIndex()) {
					return;
				}
				if (!hostiles.Contains(hostile)) {
					// TODO: no damage for eye enemy bug!
					hostile.ReceiveDamage(info.ownerID, damage);
					hostiles.Add(hostile);
				}
			}
		}
	}
}