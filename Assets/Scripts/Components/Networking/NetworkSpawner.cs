using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class NetworkSpawner : NetworkBehaviour
	{
		public GameObject prefab;
		public int count = 3;
		public float radius = 3;
		[Range(0, 60)] public float respawnDelay = 10;

		struct Instance
		{
			public GameObject gameObject;
			public float timer;
		}

		Instance[] instances;

		public override void OnStartServer() {
			instances = new Instance[count];
		}

		[Server]
		void Update() {
			for (int i = 0; i < instances.Length; i++) {
				if (instances[i].gameObject) {
					continue;
				}
				if (instances[i].timer > 0) {
					instances[i].timer = Mathf.Max(0, instances[i].timer - Time.deltaTime);
					continue;
				}
				instances[i].timer = respawnDelay;
				var gameObject = Instantiate(prefab);
				var transform = gameObject.transform;
				transform.position = (Vector2) this.transform.position + Random.insideUnitCircle * radius;
				instances[i].gameObject = gameObject;
				OnInstantiate(instances[i].gameObject);
				NetworkServer.Spawn(instances[i].gameObject);
			}
		}

		protected virtual void OnInstantiate(GameObject gameObject) {

		}

		void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}