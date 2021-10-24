using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
	public abstract class Pool : Core.Unique
	{
		public Pool_Object objectPrefab;
		public int poolSize = 50;
		public float objectMaxLifetime = 2;

		public static IEnumerable<Pool> GetAll() {
			return GetUniques().Where(x => x is Pool).Select(x => x as Pool);
		}

		public static T Get<T>(Pool_Object poolObject) where T : Pool {
			return GetAll().Where(x => x is T && x.objectPrefab == poolObject).FirstOrDefault() as T;
		}

		[Core.ReadOnly] public List<Pool_Object> localObjects = new List<Pool_Object>();

		[Core.ReadOnly, SerializeField] uint lastObjectID;

		protected uint NextObjectID() {
			return ++lastObjectID;
		}

		protected override void Awake() {
			base.Awake();
			if (!Application.isPlaying) {
				return;
			}
			for (int i = 0; i < poolSize; i++) {
				var localObject = Instantiate(objectPrefab);
				localObject.info.poolID = ID;
				localObject.transform.parent = transform;
				localObjects.Add(localObject);
			}
		}

		protected Pool_Object Spawn(byte ownerRingIndex, byte ownerFraction) {
			if (Utility.TryGetMyID(out var id)) {
				return Spawn(id, ownerRingIndex, ownerFraction, ++lastObjectID);
			}
			return null;
		}

		protected Pool_Object Spawn(uint ownerID, byte ownerRingIndex, byte ownerFraction, uint objectID) {
			foreach (var localObject in localObjects) {
				if (localObject.info.isInUse) {
					continue;
				}
				localObject.info.ownerID = ownerID;
				localObject.info.ownerRingIndex = ownerRingIndex;
				localObject.info.ownerFraction = ownerFraction;
				localObject.info.objectID = objectID;
				OnSpawn(localObject);
				localObject.gameObject.SetActive(true);
				return localObject;
			}
			return null;
		}

		protected abstract void OnSpawn(Pool_Object localObject);

		protected bool TryGetObject(Pool_ObjectInfo info, out Pool_Object localObject) {
			foreach (var o in from o in localObjects
							  where o.info.ownerID == info.ownerID
							  where o.info.objectID == info.objectID
							  select o
			) {
				localObject = o;
				return true;
			}
			localObject = null;
			return false;
		}

		public void DisableAll(uint ownerID) {
			foreach (var localObject in localObjects.Where(x => x.info.ownerID == ownerID)) {
				localObject.Disable();
			}
		}

		public void Disable(Pool_ObjectInfo info) {
			if (TryGetObject(info, out var localObject)) {
				localObject.Disable();
			}
		}

		public void Destroy(Pool_ObjectInfo info) {
			if (TryGetObject(info, out var localObject)) {
				localObject.gameObject.SetActive(false);
			}
		}

		public void DestroyAll() {
			foreach (var localObject in localObjects) {
				localObject.gameObject.SetActive(false);
			}
		}
	}
}