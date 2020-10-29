using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool : Pool
	{
		Vector3 position;
		
		public DefaultPool_Object Spawn(byte ownerRingIndex, Vector3 position) {
			this.position = position;
			return Spawn(ownerRingIndex, 0) as DefaultPool_Object;
		}
		
		public DefaultPool_Object Spawn(uint ownerID, byte ownerRingIndex, uint objectID, Vector3 position) {
			this.position = position;
			return Spawn(ownerID, ownerRingIndex, 0, objectID) as DefaultPool_Object;
		}

		protected override void OnSpawn(Pool_Object localObject) {
			(localObject as DefaultPool_Object).position.value = position;
		}

		public void UpdatePosition(Pool_ObjectInfo info, Vector3 position) {
			if (TryGetObject(info, out var localObject)) {
				(localObject as DefaultPool_Object).position.target = position;
			}
		}
	}
}