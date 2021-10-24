using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Attractor))]
	public class BatEnemy : Enemy
	{
		protected override bool ringIndexFading => true;

		[SerializeField] float aggroRadius = 10;
		[SerializeField] float homeRadius = 2;
		[SerializeField] bool aggroAggression = false;

		[SerializeField] float idleMoveSpeed = .5f;
		[SerializeField] float attackMoveSpeed = 7.5f;

		[SerializeField] ProjectilePool_Object projectilePrefab = null;
		[SerializeField] float projectileInterval = 0.5f;

		Vector2 randomPosition;

		float projectileTime;
		float alternativeAttackTime;
		Vector2 alternativeAttackDirection;

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, aggroRadius);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, homeRadius);
		}

		public override void OnStartServer() {
			base.OnStartServer();
			randomPosition = GetRandomPosition();
		}

		Vector2 GetRandomPosition() {
			return startPosition + Random.insideUnitCircle * homeRadius;
		}

		protected override void OnServerUpdate() {
			if (!isReadyAndAlive) {
				this.GetFromCache<Attractor>().mode = Attractor.Mode.Frozen;
				return;
			}
			this.GetFromCache<Attractor>().mode = Attractor.Mode.Kinematic;
			var character = NetworkManager.GetClosestCharacter(this.GetRingIndex(), transform.position, true);
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
					var direction = Utility.GetDirection2D(transform.position, character.sweetspot.position);
					Pool.Get<ProjectilePool>(projectilePrefab).Spawn(this.GetRingIndex(), GetFraction(), transform.position, direction);
					projectileTime += projectileInterval;
				}
			} else if (aggroAggression && Vector2.Distance(transform.position, character.sweetspot.position) <= aggroRadius) {
				state = State.Attack;
			}
		}

		protected override void OnFixedUpdate_Idle() {
			var distance = Vector2.Distance(transform.position, randomPosition);
			if (distance > 2) {
				this.GetFromCache<Attractor>().kinematicMovement = Utility.GetDirection2D(transform.position, randomPosition) * idleMoveSpeed * 5;
			} else if (distance > .2f) {
				this.GetFromCache<Attractor>().kinematicMovement = Utility.GetDirection2D(transform.position, randomPosition) * idleMoveSpeed;
			} else {
				randomPosition = GetRandomPosition();
			}
		}

		protected override void OnFixedUpdate_Attack() {
			var character = NetworkManager.GetClosestCharacter(this.GetRingIndex(), transform.position, true);
			if (!character) {
				return;
			}
			if (Vector2.Distance(transform.position, character.sweetspot.position) < 2) {
				alternativeAttackTime = .25f;
				alternativeAttackDirection = Utility.GetDirection2D(character.sweetspot.position, transform.position);
			}
			Vector2 direction;
			if (alternativeAttackTime > 0) {
				alternativeAttackTime -= Time.fixedDeltaTime;
				direction = alternativeAttackDirection;
			} else {
				direction = Utility.GetDirection2D(transform.position, character.sweetspot.position);
			}
			this.GetFromCache<Attractor>().kinematicMovement = direction * attackMoveSpeed;
		}
	}
}