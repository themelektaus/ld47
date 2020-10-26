using Mirror;
using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Attractor))]
	public class BatEnemy : NetworkBehaviour, IHostile
	{
		public enum State
		{
			Idle,
			Attack
		}

		[SerializeField] float health = 3;
		[SerializeField] float aggroRadius = 10;
		[SerializeField] float homeRadius = 2;
		[SerializeField] bool aggroAggression = false;

		[SerializeField] float idleMoveSpeed = .5f;
		[SerializeField] float attackMoveSpeed = 7.5f;

		[SerializeField] ProjectilePool_Object projectilePrefab = null;
		[SerializeField] float projectileInterval = 0.5f;
		[SerializeField] GameObject hitEffect = null;
		[SerializeField] Audio.SoundEffect hitSoundEffect = null;

		[SerializeField] State state = State.Idle;

		Animator animator;

		Attractor attractor;
		float currentHealth;
		Vector2 startPosition;
		Vector2 randomPosition;

		float projectileTime;
		float alternativeAttackTime;
		Vector2 alternativeAttackDirection;

		[SyncVar] public bool dead;

		public override void OnStartClient() {
			base.OnStartClient();
			animator = GetComponentInChildren<Animator>();
		}

		public override void OnStartServer() {
			base.OnStartServer();
			attractor = GetComponent<Attractor>();
			currentHealth = health;
			startPosition = transform.position;
			randomPosition = GetRandomPosition();
		}

		Vector2 GetRandomPosition() {
			return startPosition + Random.insideUnitCircle * homeRadius;
		}

		void Update() {
			if (isClient) {
				animator.SetBool("Dead", dead);
				return;
			}
			if (dead) {
				attractor.mode = Attractor.Mode.Frozen;
				return;
			} else {
				attractor.mode = Attractor.Mode.Kinematic;
			}
			var player = NetworkManager.GetClosestPlayer(transform.position);
			if (!player) {
				if (state != State.Idle) {
					state = State.Idle;
					randomPosition = GetRandomPosition();
				}
				return;
			}
			if (state == State.Attack) {
				if (projectileTime > 0) {
					projectileTime -= Time.deltaTime;
				} else {
					var direction = Utils.GetDirection2D(transform.position, player.sweetspot.position);
					Pool.Get<ProjectilePool>(projectilePrefab).Spawn(transform.position, direction);
					projectileTime += projectileInterval;
				}
			} else if (aggroAggression && Vector2.Distance(transform.position, player.sweetspot.position) <= aggroRadius) {
				state = State.Attack;
			}
		}

		[ServerCallback]
		void FixedUpdate() {
			if (dead) {
				return;
			}
			Vector2 direction;
			switch (state) {
				case State.Idle:
					if (Vector2.Distance(transform.position, randomPosition) > .2f) {
						attractor.kinematicMovement = Utils.GetDirection2D(transform.position, randomPosition) * idleMoveSpeed;
					} else {
						randomPosition = GetRandomPosition();
					}
					break;
				case State.Attack:
					var player = NetworkManager.GetClosestPlayer(transform.position);
					if (!player) {
						break;
					}
					if (Vector2.Distance(transform.position, player.sweetspot.position) < 2) {
						alternativeAttackTime = .25f;
						alternativeAttackDirection = Utils.GetDirection2D(player.sweetspot.position, transform.position);
					}
					if (alternativeAttackTime > 0) {
						alternativeAttackTime -= Time.fixedDeltaTime;
						direction = alternativeAttackDirection;
					} else {
						direction = Utils.GetDirection2D(transform.position, player.sweetspot.position);
					}
					attractor.kinematicMovement = direction * attackMoveSpeed;
					break;
			}
		}

		public void ReceiveDamage(uint senderID, float damage) {
			if (isServer) {
				TakeDamageData(damage);
				RPC_TakeDamageFX(0);
				if (Utils.TryGetConnection(senderID, out var connection)) {
					TargetRPC_CameraShake(connection);
				}
				return;
			}
			TakeDamage(senderID, damage);
			TakeDamageFX();
			CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		}

		[TargetRpc]
		void TargetRPC_CameraShake(NetworkConnection target) {
			this.Log("[TargetRpc] TargetRPC_CameraShake(...)");
			CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		}

		[Command(ignoreAuthority = true)]
		void TakeDamage(uint senderID, float damage) {
			TakeDamageData(damage);
			RPC_TakeDamageFX(senderID);
		}

		void TakeDamageData(float damage) {
			state = State.Attack;
			currentHealth -= damage;
			if (currentHealth <= 0) {
				dead = true;
				StartCoroutine(DestroyRoutine());
			}
		}

		void TakeDamageFX() {
			hitSoundEffect.Play(this);
			Instantiate(hitEffect)
				.ToTempInstance()
				.transform.position = transform.position;
		}

		[ClientRpc]
		void RPC_TakeDamageFX(uint senderID) {
			if (!Utils.IsMine(senderID)) {
				TakeDamageFX();
			}
		}

		IEnumerator DestroyRoutine() {
			yield return new WaitForSeconds(1);
			NetworkServer.Destroy(gameObject);
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, aggroRadius);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, homeRadius);
		}
	}
}