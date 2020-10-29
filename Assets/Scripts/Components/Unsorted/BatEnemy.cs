using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Attractor))]
	public class BatEnemy : Enemy
	{
		[SerializeField] float aggroRadius = 10;
		[SerializeField] float homeRadius = 2;
		[SerializeField] bool aggroAggression = false;

		[SerializeField] float idleMoveSpeed = .5f;
		[SerializeField] float attackMoveSpeed = 7.5f;

		[SerializeField] ProjectilePool_Object projectilePrefab = null;
		[SerializeField] float projectileInterval = 0.5f;
		
		Attractor attractor;
		Vector2 randomPosition;

		float projectileTime;
		float alternativeAttackTime;
		Vector2 alternativeAttackDirection;
		
		public override void OnStartServer() {
			base.OnStartServer();
			attractor = GetComponent<Attractor>();
			randomPosition = GetRandomPosition();
		}

		Vector2 GetRandomPosition() {
			return startPosition + Random.insideUnitCircle * homeRadius;
		}

		protected override void OnServerUpdate() {
			if (!isReadyAndAlive) {
				attractor.mode = Attractor.Mode.Frozen;
				return;
			}
			attractor.mode = Attractor.Mode.Kinematic;
			var character = NetworkManager.GetClosestCharacter(GetRingIndex(), transform.position, true);
			if (!character) {
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
					var direction = Utils.GetDirection2D(transform.position, character.sweetspot.position);
					Pool.Get<ProjectilePool>(projectilePrefab).Spawn(GetRingIndex(), GetFraction(), transform.position, direction);
					projectileTime += projectileInterval;
				}
			} else if (aggroAggression && Vector2.Distance(transform.position, character.sweetspot.position) <= aggroRadius) {
				state = State.Attack;
			}
		}

		[ServerCallback]
		void FixedUpdate() {
			if (!isReadyAndAlive) {
				return;
			}
			Vector2 direction;
			switch (state) {
				case State.Idle:
					var distance = Vector2.Distance(transform.position, randomPosition);
					if (distance > 2) {
						attractor.kinematicMovement = Utils.GetDirection2D(transform.position, randomPosition) * idleMoveSpeed * 5;
					} else if (distance > .2f) {
						attractor.kinematicMovement = Utils.GetDirection2D(transform.position, randomPosition) * idleMoveSpeed;
					} else {
						randomPosition = GetRandomPosition();
					}
					break;
				case State.Attack:
					var character = NetworkManager.GetClosestCharacter(GetRingIndex(), transform.position, true);
					if (!character) {
						break;
					}
					if (Vector2.Distance(transform.position, character.sweetspot.position) < 2) {
						alternativeAttackTime = .25f;
						alternativeAttackDirection = Utils.GetDirection2D(character.sweetspot.position, transform.position);
					}
					if (alternativeAttackTime > 0) {
						alternativeAttackTime -= Time.fixedDeltaTime;
						direction = alternativeAttackDirection;
					} else {
						direction = Utils.GetDirection2D(transform.position, character.sweetspot.position);
					}
					attractor.kinematicMovement = direction * attackMoveSpeed;
					break;
			}
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, aggroRadius);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, homeRadius);
		}
	}
}