using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool : Pool
	{
		Vector3 position;
		
		public DefaultPool_Object Spawn(Vector3 position) {
			this.position = position;
			return Spawn() as DefaultPool_Object;
		}
		
		public DefaultPool_Object Spawn(uint ownerID, uint objectID, Vector3 position) {
			this.position = position;
			return Spawn(ownerID, objectID) as DefaultPool_Object;
		}

		protected override void OnSpawn(Pool_Object localObject) {
			(localObject as DefaultPool_Object).transform.position = position;
		}
	}
}