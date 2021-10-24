using Mirror;
using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	[ExecuteInEditMode]
	public class EyeEnemy : Enemy
	{
		protected override bool ringIndexFading => false;

		public Transform target;
		public Transform eye;
		public Transform hole1;
		public Transform hole2;
		public Vector2 offset = new Vector2(.15f, 0.08f);
		public Vector2 scale = new Vector2(.1f, .1f);
		public float distanceFactor = 10;
		public ProjectilePool_Object projectilePrefab;

		[Core.Attributes.ReadOnly] public Vector2 offset1;
		[Core.Attributes.ReadOnly] public Vector2 offset2;

		SmoothTransformLocalPosition eyePosition;
		Timer attackTimer = 3;
		Timer targetTimer = 2;

		protected override void Awake() {
			base.Awake();
			eyePosition = (eye, .2f);
		}

		void OnDrawGizmos() {
			if (target && eye) {
				Gizmos.color = Color.red;
				Gizmos.DrawLine(eye.position, target.position);
			}
		}

		protected override void OnServerUpdate() {
			if (Application.isPlaying && NetworkServer.active) {
				if (!target) {
					targetTimer.Reset();
				}
				if (targetTimer.Update()) {
					var targetCharacter = NetworkManager.GetClosestCharacter(0, transform.position, true);
					target = targetCharacter ? targetCharacter.transform : null;
				}
			}
			if (!target) {
				if (eye) {
					eye.localPosition = Vector3.zero;
					eye.localScale = Vector3.one;
				}
				if (hole1) {
					hole1.localPosition = Vector3.zero;
					hole1.localScale = Vector3.one;
				}
				if (hole2) {
					hole2.localPosition = Vector3.zero;
					hole2.localScale = -Vector3.one;
				}
				return;
			}
			offset1 = target.position - transform.position;
			var distance = offset1.magnitude;
			var direction = offset1.normalized;
			offset2 = Mathf.Min(1, distance / distanceFactor) * direction;
			UpdateEye();
			UpdateHoles();
			if (state == State.Attack) {
				if (attackTimer.Update()) {
					Server_Attack();
				}
			}
		}

		void UpdateEye() {
			if (!eye) {
				return;
			}
			eyePosition.target = offset2 * offset;
			eyePosition.Update();
			eye.localScale = new Vector3(1 - Mathf.Abs(offset2.x * scale.x), 1 - Mathf.Abs(offset2.y * scale.y), 1);
		}

		void UpdateHoles() {
			if (!hole1 || !hole2) {
				return;
			}
			hole1.localPosition = new Vector3(0, offset2.y * scale.y, 0) / 2;
			hole1.localScale = Vector3.one - new Vector3(0, offset2.y * scale.y, 0);
			hole2.localPosition = new Vector3(0, offset2.y * scale.y, 0) / 2;
			hole2.localScale = -Vector3.one - new Vector3(0, offset2.y * scale.y, 0);
		}

		[Server]
		void Server_Attack() {
			this.GetFromCache<NetworkAnimator>().SetTrigger("Attack");
		}

		public void Shoot() {
			var position = eye.transform.position;
			position.z = -1;
			Pool.Get<ProjectilePool>(projectilePrefab).Spawn(this.GetRingIndex(), GetFraction(), position, offset1.normalized, new Vector3(3, 11.2f, 3));
		}

		protected override void OnFixedUpdate_Idle() {
			
		}

		protected override void OnFixedUpdate_Attack() {
			
		}
	}
}