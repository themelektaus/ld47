using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool : Pool
	{
		Vector3 position;
		Vector2 direction;
		Vector3 scale;

		public ProjectilePool_Object Spawn(byte ownerRingIndex, byte ownerFraction, Vector3 position, Vector2 direction) {
			return Spawn(ownerRingIndex, ownerFraction, position, direction, Vector3.one);
		}

		public ProjectilePool_Object Spawn(byte ownerRingIndex, byte ownerFraction, Vector3 position, Vector2 direction, Vector3 scale) {
			this.position = position;
			this.direction = direction;
			this.scale = scale;
			return Spawn(ownerRingIndex, ownerFraction) as ProjectilePool_Object;
		}

		public ProjectilePool_Object Spawn(uint ownerID, byte ownerRingIndex, byte ownerFraction, uint objectID, Vector3 position, Vector2 direction) {
			return Spawn(ownerID, ownerRingIndex, ownerFraction, objectID, position, direction, Vector3.one);
		}

		public ProjectilePool_Object Spawn(uint ownerID, byte ownerRingIndex, byte ownerFraction, uint objectID, Vector3 position, Vector2 direction, Vector3 scale) {
			this.position = position;
			this.direction = direction;
			this.scale = scale;
			return Spawn(ownerID, ownerRingIndex, ownerFraction, objectID) as ProjectilePool_Object;
		}

		protected override void OnSpawn(Pool_Object localObject) {
			var projectile = localObject as ProjectilePool_Object;
			projectile.position.value = position;
			projectile.direction = direction;
			projectile.transform.localScale = scale;
		}

		public void UpdatePosition(Pool_ObjectInfo info, Vector3 position) {
			if (TryGetObject(info, out var localObject)) {
				(localObject as ProjectilePool_Object).position.target = position;
			}
		}
	}
}