using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool : Unique
	{
		public Projectile objectPrefab;
		public int poolSize = 50;
		public float objectMaxLifetime = 2;

		[ReadOnly] public List<Projectile> localObjects = new List<Projectile>();

		[ReadOnly, SerializeField] uint lastObjectID;

		void Awake() {
			if (!Application.isPlaying) {
				return;
			}
			for (int i = 0; i < poolSize; i++) {
				var localObject = Instantiate(objectPrefab);
				localObject.pool = this;
				localObject.transform.parent = transform;
				localObjects.Add(localObject);
			}
		}

		public Projectile Instantiate(string tag, Vector3 position, Vector2 targetPosition) {
			foreach (var localObject in localObjects) {
				if (!localObject.objectID.isEmpty) {
					continue;
				}
				localObject.objectID = Projectile.ObjectID.Create(Mirror.NetworkClient.connection.identity.netId, ++lastObjectID);
				localObject.tag = tag;
				localObject.direction = Utils.GetDirection2D(position, targetPosition);
				var a = localObject.transform.position;
				var b = position;
				a.x = b.x;
				a.y = b.y;
				localObject.transform.position = a;
				localObject.gameObject.SetActive(true);
				return localObject;
			}
			return null;
		}
	}
}