using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MT.Packages.LD47
{
    [RequireComponent(typeof(Character))]
    public class CharacterBot : MonoBehaviour
    {
		public struct Target
		{
			public Enemy enemy;
			public Waypoint waypoint;
			public Character character;
			public System.Func<Transform, bool> isValid;
			public Transform GetTransform() {
				if (enemy) return enemy.transform;
				if (waypoint) return waypoint.transform;
				if (character) return character.transform;
				return null;
			}
		}

		[Core.Attributes.ReadOnly] public Character character;
		public float moveSpeed = 1;

		public Target target;
		[Core.Attributes.ReadOnly] public float horizontal;
		[Core.Attributes.ReadOnly] public float jumpTime;

		public readonly Core.Timer horizontalTimer = (1, 1.5f);
		public readonly Core.Timer targetTimer = .2f;
		public readonly Core.Timer jumpTimer = (2.5f, 4.2f);

		readonly Core.Timer shootTimer = (.34f, .68f);
		readonly Core.Timer shootHoldTimer = (.3f, 5f);
		bool shooting;

		Coroutine respawnCoroutine;
		Vector3 startPosition;
		byte startRingIndex;

		void ResetBot() {
			horizontal = 0;
			jumpTime = 0;
			horizontalTimer.Reset();
			targetTimer.Reset();
			jumpTimer.Reset();
			target = new Target();
		}

		void Awake() {
			character = GetComponent<Character>();
			startPosition = transform.position;
			startRingIndex = character.GetRingIndex();
		}

		[ServerCallback]
		void Start() {
			character.Server_SetBot(true);
			character.Respawn(startPosition, startRingIndex);
		}
		
		[ServerCallback]
		void Update() {
			if (character.isDead) {
				Server_StartRespawnCoroutine();
			} else {
				Server_UpdateBot();
			}
		}

		[Server]
		void Server_StartRespawnCoroutine() {
			if (respawnCoroutine != null) {
				return;
			}
			ResetBot();
			IEnumerator RespawnRoutine() {
				yield return new WaitForSeconds(5);
				character.Respawn(startPosition, startRingIndex);
				respawnCoroutine = null;
			}
			respawnCoroutine = StartCoroutine(RespawnRoutine());
		}

		// [Server]
		// void Server_StopRespawnCoroutine() {
		// 	if (respawnCoroutine != null) {
		// 		StopCoroutine(respawnCoroutine);
		// 		respawnCoroutine = null;
		// 	}
		// }

		[Server]
		void Server_UpdateBot() {
			if (targetTimer.Update()) {
				Server_UpdateTarget();
			}
			if (target.enemy) {
				character.aimPosition.value = target.enemy.transform.position;
			} else {
				var offset = character.inputHorizontal > 0 ? transform.right : -transform.right;
				offset += transform.up * .4f;
				offset *= 4;
				character.aimPosition.value = transform.position + offset;
			}
			if (horizontalTimer.Update()) {
				if (target.enemy) {
					horizontal = Random.value * 2 - 1;
				} else if (target.waypoint) {
					var w = target.waypoint;
					horizontal = Utility.GetHorizontal(character.transform, w.GetPosition(), w.distanceDamping, .1f, .1f);
					horizontalTimer.Reset();
				} else if (target.character) {
					horizontal = Utility.GetHorizontal(character.transform, target.character.transform.position, 15, 5, .1f);
					horizontalTimer.Reset();
				} else {
					horizontal = Random.value * 2 - 1;
				}
			}
			character.inputHorizontal = horizontal * moveSpeed;
			character.inputJump = false;
			if (jumpTimer.Update()) {
				character.inputJump = true;
				var e = target.enemy;
				var w = target.waypoint;
				jumpTime = Random.value / (w ? w.jump : (e ? 1 : 10));
			}
			jumpTime = Mathf.Max(0, jumpTime - Time.deltaTime);
			character.inputJumpHold = jumpTime > 0;
			if (target.enemy && target.enemy.isReadyAndAlive) {
				Server_UpdateShoot();
			}
		}

		[Server]
		void Server_UpdateShoot() {
			var weapon = character.weaponInstance;
			if (!weapon) {
				return;
			}
			var ringIndex = character.GetRingIndex();
			var fraction = character.GetFraction();
			var targetPosition = character.aimPosition.current;
			if (weapon.IsAutomatic()) {
				if (!shooting && shootTimer.Update()) {
					shooting = true;
				} else if (shooting && shootHoldTimer.Update()) {
					shooting = false;
				}
				if (shooting) {
					weapon.Shoot(ringIndex, fraction, targetPosition);
				}
				return;
			}
			if (weapon.NeedsCast()) {
				weapon.Shoot(ringIndex, fraction, targetPosition);
				return;
			}
			if (shootTimer.Update()) {
				weapon.Shoot(ringIndex, fraction, targetPosition);
			}
		}

		[Server]
		void Server_UpdateTarget() {
			var enemy = NetworkManager.GetClosestAttackingEnemy(character.GetRingIndex(), character.transform.position, 20);
			if (enemy) {
				target.enemy = enemy;
				return;
			}
			target.enemy = null;

			var otherCharacter = NetworkManager.GetClosestCharacter(character.GetRingIndex(), character.transform.position, false, 22);
			if (otherCharacter) {
				target = new Target {
					character = otherCharacter,
					isValid = t => otherCharacter && otherCharacter.isInGameAndAlive && character.GetRingIndex() == otherCharacter.GetRingIndex()
				};
				return;
			}

			if (target.isValid != null && target.isValid(target.GetTransform())) {
				return;
			}

			foreach (var waypoint in FindObjectsOfType<Waypoint>()
				.Where(x => x.GetRingIndex() == character.GetRingIndex())
				.OrderBy(x => (x.GetPosition() - (Vector2) character.transform.position).sqrMagnitude)
			) {
				Server_SetTarget(waypoint);
				return;
			}
		}

		[Server]
		void Server_SetTarget(Waypoint waypoint) {
			target = new Target {
				waypoint = waypoint,
				isValid = t => {
					if (t && t.TryGetComponent<Waypoint>(out var w)) {
						var r = t.GetComponentInChildren<ChangeRing>();
						if (r) {
							if (character.GetRingIndex() == r.GetRingIndex()) {
								Server_SetTarget(w.GetNextWaypoint());
								return true;
							}
						} else if (Vector2.Distance(character.transform.position, w.GetPosition()) < w.radius) {
							Server_SetTarget(w.GetNextWaypoint());
							return true;
						}
						if (character.GetRingIndex() != w.GetRingIndex()) {
							return false;
						}
					}
					return true;
				}
			};
		}
	}
}