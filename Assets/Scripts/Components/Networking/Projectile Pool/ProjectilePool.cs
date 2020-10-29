using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool : Pool
	{
		Vector3 position;
		Vector2 direction;

		public ProjectilePool_Object Spawn(byte ownerRingIndex, byte ownerFraction, Vector3 position, Vector2 direction) {
			this.position = position;
			this.direction = direction;
			return Spawn(ownerRingIndex, ownerFraction) as ProjectilePool_Object;
		}

		public ProjectilePool_Object Spawn(uint ownerID, byte ownerRingIndex, byte ownerFraction, uint objectID, Vector3 position, Vector2 direction) {
			this.position = position;
			this.direction = direction;
			return Spawn(ownerID, ownerRingIndex, ownerFraction, objectID) as ProjectilePool_Object;
		}

		protected override void OnSpawn(Pool_Object localObject) {
			(localObject as ProjectilePool_Object).position.value = position;
			(localObject as ProjectilePool_Object).direction = direction;
		}

		public void UpdatePosition(Pool_ObjectInfo info, Vector3 position) {
			if (TryGetObject(info, out var localObject)) {
				(localObject as ProjectilePool_Object).position.target = position;
			}
		}
	}
}