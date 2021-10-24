using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_LaserObject : ProjectilePool_Object
	{
		public override void OnEnableObject() {
			base.OnEnableObject();
			this.GetFromCache<Collider2D>().enabled = true;
		}

		public override void OnUpdate() {
			base.OnUpdate();
			var scale = transform.localScale;
			scale.x -= Time.deltaTime * 3;
			if (scale.x < 0) {
				scale.x = 0;
			}
			transform.localScale = scale;
		}

		protected override void OnTrigger(Collider2D collision) {
			OnTriggerBase(collision);
			if (OnTriggerHostile(collision) == HitResult.Hostile) {
				this.GetFromCache<Collider2D>().enabled = false;
			}
		}

		public override void OnNetworkAdd() {
			Utility.Send((ProjectilePool_Message_AddWithScale) (info, position.current, direction, transform.localScale));
		}
	}
}