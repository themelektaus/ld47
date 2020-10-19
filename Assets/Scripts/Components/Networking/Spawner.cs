using System.Collections;
using System.Linq;
using TNet;
using UnityEngine;
using G = System.Collections.Generic;

#if UNITY_EDITOR
using H = UnityEditor.Handles;
#endif

namespace MT.Packages.LD47
{
	public class Spawner : Unique
	{
		public static G.List<Spawner> spawners = new G.List<Spawner>();
		public static G.List<SpawnerObject> spawnerObjects = new G.List<SpawnerObject>();

		public static T GetClosestSpawnerObject<T>(Vector2 position) where T : SpawnerObject {
			return spawnerObjects
				.Where(x => x is T && x.isActiveAndEnabled)
				.Select(spawnerObject => {
					float distance = Vector2.Distance(position, spawnerObject.transform.position);
					return (spawnerObject, distance);
				})
				.OrderBy(x => x.distance)
				.FirstOrDefault()
				.spawnerObject as T;
		}

		public static T GetRandomSpawnerObject<T>() where T : SpawnerObject {
			var list = spawnerObjects.Where(x => x is T && (!(x is IHostile y) || !y.IsDead()) && x.isActiveAndEnabled).ToList();
			if (list.Count == 0) {
				return null;
			}
			return list[Random.Range(0, list.Count)] as T;
		}

		[System.Serializable]
		public class Object
		{
			[ResourcePath("prefab")] public string prefab;
			public float respawnDelay;
			public Vector2 offset;
			public float radius;

			[System.NonSerialized] public Spawner spawner;

			public Vector3 GetPosition() {
				var position = (Vector2) spawner.transform.position + offset;
				if (radius != 0) {
					position += Random.insideUnitCircle * radius;
				}
				return position;
			}
		}
		[SerializeField] G.List<Object> objects = new G.List<Object>();

		readonly Timer timer = new Timer(.5f);

		bool respawn;

		void Awake() {
			foreach (var @object in objects) {
				@object.spawner = this;
			}
		}

		protected override void OnStart() {
			if (spawners.Where(x => x.ID == ID).FirstOrDefault()) {
				Debug.LogError("Spawner-ID '" + ID + "' already exists");
				Destroy(gameObject);
			}
			spawners.Add(this);
		}

		protected override void OnUpdate() {
			if (!MultiplayerGame.isReady || !timer.Update()) {
				return;
			}
			var respawn = this.respawn;
			this.respawn = true;
			if (!MultiplayerGame.isReadyAndHosting) {
				return;
			}
			for (var i = 0; i < objects.Count; i++) {
				if (transform.Find(i.ToString())) {
					continue;
				}
				if (transform.Find($"{i} (Pending)")) {
					continue;
				}
				if (transform.Find($"{i} (Pending) *")) {
					continue;
				}
				SpawnPlaceholder(i, transform);
				if (!respawn) {
					Spawn(objects[i], i);
					continue;
				}
				StartCoroutine(RespawnRoutine(objects[i], i));
			}
		}

		static void SpawnPlaceholder(int index, Transform transform) {
			new GameObject($"{index} (Pending)").transform.parent = transform;
		}

		IEnumerator RespawnRoutine(Object @object, int index) {
			yield return new WaitForSeconds(@object.respawnDelay);
			Spawn(@object, index);
		}

		void Spawn(Object @object, int index) {
			TNManager.Instantiate(MultiplayerGame.instance.channelID, nameof(RCC_Create), @object.prefab, true, index, ID, @object.GetPosition());
		}

		[RCC]
		public static GameObject RCC_Create(GameObject prefab, int index, string spawnerID, Vector3 position) {
			var gameObject = prefab.Instantiate();
			gameObject.name = index.ToString();
			var transform = gameObject.transform;
			var spawner = spawners.Where(x => x.ID == spawnerID).FirstOrDefault();
			if (spawner) {
				transform.parent = spawner.transform;
				if (MultiplayerGame.isReadyAndHosting) {
					var placeholder = transform.parent.Find($"{index} (Pending)");
					placeholder.name += " *";
					Destroy(placeholder.gameObject);
				}
			}
			transform.position = position;
			return gameObject;
		}

#if UNITY_EDITOR
		void OnDrawGizmos() {
			Gizmos.color = Color.red;
			foreach (var @object in objects) {
				var position = transform.position + (Vector3) @object.offset;
				if (@object.radius == 0) {
					float s = .25f;
					var a = position + new Vector3(-s, -s);
					var b = position + new Vector3(s, s);
					var c = position + new Vector3(s, -s);
					var d = position + new Vector3(-s, s);
					H.DrawBezier(a, b, a, b, Color.red, null, 6);
					H.DrawBezier(c, d, c, d, Color.red, null, 6);
				} else {
					Gizmos.DrawWireSphere(position, @object.radius);
				}
			}
		}
#endif
	}
}