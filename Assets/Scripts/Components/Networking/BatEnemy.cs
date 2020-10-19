using TNet;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Attractor))]
	public class BatEnemy : SpawnerObject, IHostile
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

		[SerializeField] Projectile projectilePrefab = null;
		[SerializeField] float projectileInterval = 0.5f;
		[SerializeField] GameObject hitEffect = null;
		[SerializeField] Audio.SoundEffect hitSoundEffect = null;

		[SerializeField] State state = State.Idle;

		Attractor attractor;
		Timer timer;
		float currentHealth;
		Animator animator;

		SmoothVector3 position;
		Vector2 startPosition;
		Vector2 randomPosition;

		float projectileTime;
		float alternativeAttackTime;
		Vector2 alternativeAttackDirection;

		protected override void Awake() {
			base.Awake();
			attractor = GetComponent<Attractor>();
			timer = new Timer(.4f);
			animator = GetComponentInChildren<Animator>();
		}

		protected override void OnHostAwake() {
			currentHealth = health;
		}

		public override void OnStart() {
			base.OnStart();
			startPosition = transform.position;
			randomPosition = GetRandomPosition();
			position = new SmoothVector3(() => transform.position, x => transform.position = x, .5f);
		}

		Vector2 GetRandomPosition() {
			return startPosition + Random.insideUnitCircle * homeRadius;
		}

		protected override void OnRemoteUpdate() {
			attractor.state = Attractor.State.Frozen;
			position.Update();
		}

		protected override void OnHostUpdate() {
			if (timer.Update()) {
				this.Send(nameof(RFC_UpdateRemoteData), Target.Others, transform.position, currentHealth);
			}
			if (IsDead()) {
				attractor.state = Attractor.State.Frozen;
				return;
			} else {
				attractor.state = Attractor.State.Kinematic;
			}
			var player = Player.GetClosestPlayer(transform.position);
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
					if (NetworkPool.TryGet<Projectile, ProjectilePool>(projectilePrefab, out var pool)) {
						pool.Instantiate(tag, transform.position, player.sweetspot.position);
					}
					projectileTime += projectileInterval;
				}
			} else if (aggroAggression && Vector2.Distance(transform.position, player.sweetspot.position) <= aggroRadius) {
				state = State.Attack;
			}
		}

		protected override void OnFixedHostUpdate() {
			base.OnFixedHostUpdate();
			if (IsDead()) {
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
					var player = Player.GetClosestPlayer(transform.position);
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

		public bool IsDead() {
			return animator.GetBool("Dead");
		}

		public void SetDead() {
			if (IsDead()) {
				return;
			}
			animator.SetBool("Dead", true);
		}

		public void ReceiveDamage(int senderID, string senderTag, float damage) {
			if (damage == 0) {
				return;
			}
			this.Send(nameof(RFC_TakeDamage), Target.Host, damage);
			this.Send(nameof(RFC_TakeDamageFX), Target.All);
			this.Send(nameof(RFC_TakeDamageSound), senderID);
		}

		[RFC]
		void RFC_UpdateRemoteData(Vector3 targetPosition, float currentHealth) {
			if (!position) {
				return;
			}
			position.target = targetPosition;
			this.currentHealth = currentHealth;
			if (currentHealth <= 0) {
				SetDead();
			}
		}

		[RFC]
		void RFC_TakeDamage(float damage) {
			state = State.Attack;
			currentHealth -= damage;
			if (currentHealth <= 0) {
				SetDead();
				gameObject.DestroySelf(1);
			}
		}

		[RFC]
		void RFC_TakeDamageFX() {
			var effect = Instantiate(hitEffect).ToTempInstance();
			effect.transform.position = transform.position;
		}

		[RFC]
		void RFC_TakeDamageSound() {
			hitSoundEffect.Play(this);
			CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, aggroRadius);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, homeRadius);
		}
	}
}