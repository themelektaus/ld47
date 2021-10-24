using Mirror;
using MT.Packages.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	[ExecuteAlways]
	[RequireComponent(typeof(ObjectInfo))]
	public class NetworkSpawner : NetworkBehaviour
	{
		public bool requiresClientConnection = false;
		public GameObject prefab;
		public int count = 3;
		public float radius = 3;
		[Range(-1, 60)] public float respawnDelay = 10;

		struct Instance
		{
			public GameObject gameObject;
			public float timer;
		}

		Instance[] instances;

		void OnEnable() {
			transform.DestroyChildrenImmediate();
			Hash.Clear(this);
		}

		void OnDisable() {
			transform.DestroyChildrenImmediate();
			Hash.Clear(this);
		}

		public override void OnStartServer() {
			instances = new Instance[count];
		}

		void Update() {
			if (!Application.IsPlaying(gameObject)) {
				UpdateEditor();
				return;
			}
			if (!isServer) {
				return;
			}
			for (int i = 0; i < instances.Length; i++) {
				if (requiresClientConnection && NetworkServer.connections.Count == 0) {
					instances[i].timer = 0;
					if (instances[i].gameObject) {
						NetworkServer.Destroy(instances[i].gameObject);
					}
					continue;
				}
				if (instances[i].gameObject) {
					continue;
				}
				if (instances[i].timer < 0) {
					continue;
				}
				if (instances[i].timer > 0) {
					instances[i].timer = Mathf.Max(0, instances[i].timer - Time.deltaTime);
					continue;
				}
				instances[i].timer = respawnDelay;
				var gameObject = Instantiate(prefab);
				var transform = gameObject.transform;
				var position = (Vector2) this.transform.position;
				if (radius != 0) {
					position += Random.insideUnitCircle * radius;
				}
				transform.position = position;
				instances[i].gameObject = gameObject;
				OnInstantiate(instances[i].gameObject);
				NetworkServer.Spawn(instances[i].gameObject);
			}
		}

		void UpdateEditor() {
			var spriteRenderers = prefab ? prefab.GetChildrenFromCache<SpriteRenderer>() : new SpriteRenderer[0];
			if (!Hash.HasChanged(this, prefab, count, radius, spriteRenderers)) {
				return;
			}
			transform.DestroyChildrenImmediate();
			var randoms = new List<Vector2>();
			for (int i = 0; i < count; i++) {
				var random = GetRandomEditorOffset(randoms, 2);
				if (random.HasValue) {
					foreach (var spriteRenderer in spriteRenderers) {
						var spriteRendererInstance = Instantiate(spriteRenderer, this.transform);
						var transform = spriteRendererInstance.transform;
						transform.localPosition = spriteRenderer.transform.position + (Vector3) random.Value;
						transform.localRotation = spriteRenderer.transform.rotation;
						transform.localScale = spriteRenderer.transform.lossyScale;
					}
				}
			}
		}

		Vector2? GetRandomEditorOffset(List<Vector2> randoms, float minSqrMagnitude) {
			int iterations = 0;
			Vector2? result = null;
			while (!result.HasValue && iterations < 100) {
				iterations++;
				result = Random.insideUnitCircle * radius;
				foreach (var r in randoms) {
					if (minSqrMagnitude > .1f && (r - result.Value).sqrMagnitude < minSqrMagnitude) {
						result = null;
						break;
					}
				}
			}
			if (result.HasValue) {
				randoms.Add(result.Value);
				return result;
			}
			return GetRandomEditorOffset(randoms, minSqrMagnitude / 1.25f);
		}

		protected virtual void OnInstantiate(GameObject gameObject) {

		}

		void OnDrawGizmos() {
			if (radius == 0) {
				Gizmos.color = new Color(1, .5f, 0, .5f);
				Gizmos.DrawWireSphere(transform.position, 1);
				Gizmos.DrawWireSphere(transform.position, 1.2f);
				return;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}