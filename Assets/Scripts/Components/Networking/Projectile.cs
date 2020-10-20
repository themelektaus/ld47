using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class Projectile : MonoBehaviour
	{
		[System.Serializable]
		public struct ObjectID
		{
			public uint ownerID;
			public uint id;

			public bool isEmpty { get { return ownerID == 0 && id == 0; } }

			public static ObjectID Create(uint ownerID, uint id) {
				return new ObjectID { ownerID = ownerID, id = id };
			}

			public void Clear() {
				ownerID = 0;
				id = 0;
			}

			public override string ToString() {
				return $"ownerID: {ownerID}, id: {id}";
			}
		}

		public class SpawnMessage : Mirror.MessageBase
		{
			public ObjectID objectID;

			public static void SendToServer(ObjectID objectID) {
				Mirror.NetworkClient.Send(new SpawnMessage { objectID = objectID });
			}
		}

		public ProjectilePool pool;
		public ObjectID objectID;

		[SerializeField, Range(0, 10)] float damage = 1;
		[SerializeField, Range(1, 50)] float speed = 20;
		[SerializeField] GameObject sprites = null;
		[SerializeField] ParticleSystem[] trailEffects = null;

		public Vector2 direction;
		
		[SerializeField, ResourcePath("prefab")] string explosionPrefab = null;

		void OnEnable() {
			if (!pool) {
				gameObject.SetActive(false);
				return;
			}
			StartCoroutine(DisableRoutine());
			Update();
			SpawnMessage.SendToServer(objectID);
		}

		IEnumerator DisableRoutine() {
			yield return new WaitForSeconds(pool.objectMaxLifetime);
			gameObject.SetActive(false);
		}

		void OnDisable() {
			StopAllCoroutines();
			objectID.Clear();
		}

		void Update() {
			transform.eulerAngles = new Vector3(0, 0, Utils.GetAngle2D(direction));
			var position = (Vector2) transform.position + direction * Time.deltaTime * speed;
			transform.position = new Vector3(position.x, position.y, transform.position.z);
		}

		void OnTriggerEnter2D(Collider2D collision) {
			OnTrigger(collision);
		}

		void OnTriggerStay2D(Collider2D collision) {
			OnTrigger(collision);
		}

		void OnTrigger(Collider2D collision) {
			if (collision.TryGetComponent<Projectile>(out _)) {
				return;
			}
			if (collision.TryGetComponent<IHostile>(out var _)) {
				return;
			}
			// if (collision.TryGetComponent<IHostile>(out var hostile)) {
			// 	if (hostile.IsDead()) {
			// 		return;
			// 	}
			// 	hostile.ReceiveDamage(tag, damage);
			// }
			// if (!string.IsNullOrEmpty(explosionPrefab)) {
			// 	// InstantiateExplosion();
			// }
			gameObject.SetActive(false);
			// Destroy(gameObject);
			// Destroy();
		}

		// [Command]
		// void Destroy() {
		// 	NetworkServer.Destroy(gameObject);
		// }

		// [RCC]
		// public static GameObject RCC_CreateExplosion(GameObject prefab, int playerID, string tag, Vector3 position) {
		// 	var gameObject = prefab.Instantiate().ToTempInstance();
		// 	gameObject.transform.position = position;
		// 	gameObject.tag = tag;
		// 	if (gameObject.TryGetComponent<Explosion>(out var explosion)) {
		// 		explosion.playerID = playerID;
		// 	}
		// 	return gameObject;
		// }
	}
}