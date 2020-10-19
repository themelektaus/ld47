using TNet;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class Projectile : NetworkPoolObject
	{
		[SerializeField, Range(0, 10)] float damage = 1;
		[SerializeField, Range(1, 50)] float speed = 20;
		[SerializeField] GameObject sprites = null;
		[SerializeField] ParticleSystem[] trailEffects = null;

		[SerializeField] SmoothVector3 position;
		[SerializeField, Range(0.5f, 3)] public float smoothnessFactor = 1.5f;
		[SerializeField, Range(0, 10)] public float predictionFactor = 3.5f;
		[SerializeField] Vector2 direction;

		[SerializeField, ResourcePath("prefab")] string explosionPrefab = null;

		public override void OnInit() {
			position = new SmoothVector3(
				() => transform.position,
				x => transform.position = x,
				pool.syncInterval * smoothnessFactor
			);
		}

		protected override void OnUpdate() {
			transform.eulerAngles = new Vector3(0, 0, Utils.GetAngle2D(direction));
			if (IsMine()) {
				var position = (Vector2) transform.position + direction * Time.deltaTime * speed;
				transform.position = new Vector3(position.x, position.y, transform.position.z);
			} else {
				position.Update();
				transform.position += (Vector3) direction * pool.syncInterval * predictionFactor;
			}
		}

		void OnTriggerEnter2D(Collider2D collision) {
			OnTrigger(collision);
		}

		void OnTriggerStay2D(Collider2D collision) {
			OnTrigger(collision);
		}

		void OnTrigger(Collider2D collision) {
			if (!IsMine() || collision.isTrigger) {
				return;
			}
			if (collision.TryGetComponent<Projectile>(out _)) {
				return;
			}
			if (CompareTag(collision.tag)) {
				if (collision.TryGetComponent<Player>(out var player)) {
					if (playerID == player.tno.ownerID) {
						return;
					}
				} else {
					return;
				}
			}
			if (collision.TryGetComponent<IHostile>(out var hostile)) {
				if (hostile.IsDead()) {
					return;
				}
				hostile.ReceiveDamage(playerID, tag, damage);
			}
			if (!string.IsNullOrEmpty(explosionPrefab)) {
				InstantiateExplosion();
			}
			Destroy();
		}

		public override void OnInstantiate(object[] data) {
			StopAllCoroutines();
			gameObject.SetActive(true);
			position.value = (Vector3) data[0];
			direction = (Vector2) data[1];
			sprites.SetActive(true);
			foreach (var trailEffect in trailEffects) {
				trailEffect.gameObject.SetActive(true);
				trailEffect.Play();
			}
		}

		public override object[] PrepareData() {
			return new object[] { transform.position };
		}

		public override void OnReceiveData(object[] data) {
			position.target = (Vector3) data[0];
		}

		public override void OnRelease() {
			sprites.SetActive(false);
			foreach (var trailEffect in trailEffects) {
				trailEffect.Stop();
			}
			StartCoroutine(SetInactiveRoutine());
		}

		System.Collections.IEnumerator SetInactiveRoutine() {
			yield return new WaitForSeconds(2);
			gameObject.SetActive(false);
		}

		public void InstantiateExplosion() {
			TNManager.Instantiate(MultiplayerGame.instance.channelID, nameof(RCC_CreateExplosion), explosionPrefab, false, playerID, tag, transform.position);
		}

		[RCC]
		public static GameObject RCC_CreateExplosion(GameObject prefab, int playerID, string tag, Vector3 position) {
			var gameObject = prefab.Instantiate().ToTempInstance();
			gameObject.transform.position = position;
			gameObject.tag = tag;
			if (gameObject.TryGetComponent<Explosion>(out var explosion)) {
				explosion.playerID = playerID;
			}
			return gameObject;
		}
	}
}