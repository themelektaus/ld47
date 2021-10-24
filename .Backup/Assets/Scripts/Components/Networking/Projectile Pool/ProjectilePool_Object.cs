using MT.Packages.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Object : Pool_Object
	{
		protected enum HitResult
		{
			None,
			Hostile,
			Other
		}

		public SmoothTransformPosition position;
		public Vector2 direction;

		[SerializeField, Range(0, 10)] float damage = 1;
		[SerializeField, Range(0, 50)] float speed = 20;
		[SerializeField] GameObject sprites = null;
		[SerializeField] ParticleSystem[] trailEffects = null;
		[SerializeField, Range(0, 1)] float predictionFactor = .15f;
		[SerializeField] DefaultPool_Object explosion = null;

		event System.EventHandler DamageEvent;
		readonly List<System.EventHandler> damageEventHandlers = new List<System.EventHandler>();

		public event System.EventHandler Damage {
			add { DamageEvent += value; damageEventHandlers.Add(value); }
			remove { DamageEvent -= value; damageEventHandlers.Remove(value); }
		}

		void ClearDamageEventHandlers() {
			foreach (var handler in damageEventHandlers) {
				DamageEvent -= handler;
			}
			damageEventHandlers.Clear();
		}

		void Awake() {
			position = (transform, predictionFactor);
		}

		public override void OnEnableObject() {
			sprites.SetActive(true);
			foreach (var trailEffect in trailEffects) {
				trailEffect.Play();
			}
		}

		public override void OnDisableObject() {
			ClearDamageEventHandlers();
			sprites.SetActive(false);
			foreach (var trailEffect in trailEffects) {
				trailEffect.Stop();
			}
		}

		public override void OnUpdate() {
			transform.eulerAngles = new Vector3(0, 0, Utility.GetAngle2D(direction));
			if (info.isMine) {
				var movement = direction * Time.deltaTime * speed;
				position.value = new Vector3(
					position.current.x + movement.x,
					position.current.y + movement.y,
					position.current.z
				);
			} else {
				position.Update();
			}
		}

		protected void OnTriggerBase(Collider2D collision) {
			base.OnTrigger(collision);
		}

		protected override void OnTrigger(Collider2D collision) {
			OnTriggerBase(collision);
			if (OnTriggerHostile(collision) != HitResult.None) {
				UnSpawn();
			}
		}

		protected HitResult OnTriggerHostile(Collider2D collision) {
			if (collision.HasComponent<ProjectilePool_Object>()) {
				return HitResult.None;
			}
			var hostileHitted = false;
			var hostile = collision.GetParentFromCache<IHostile>(true);
			if (hostile != null) {
				if (info.ownerRingIndex != (hostile as Component).GetRingIndex()) {
					return HitResult.None;
				}
				if (info.ownerFraction == hostile.GetFraction()) {
					return HitResult.None;
				}
				if (hostile is Character character && character.isInGameAndAlive) {
					if (damage > 0) {
						DamageEvent?.Invoke(this, System.EventArgs.Empty);
						character.ReceiveDamage(info.ownerID, damage);
						hostileHitted = true;
					}
				} else if (hostile is Enemy enemy && enemy.isReadyAndAlive) {
					if (enemy.state == Enemy.State.Ignore) {
						return HitResult.None;
					}
					if (damage > 0) {
						DamageEvent?.Invoke(this, System.EventArgs.Empty);
						enemy.ReceiveDamage(info.ownerID, damage);
						hostileHitted = true;
					}
				} else {
					return HitResult.None;
				}
			}
			if (explosion) {
				Pool.Get<DefaultPool>(explosion).Spawn(info.ownerRingIndex, GetPredictedPosition());
			}
			return hostileHitted ? HitResult.Hostile : HitResult.Other;
		}

		public override void OnNetworkAdd() {
			Utility.Send((ProjectilePool_Message_Add) (info, position.current, direction));
		}

		public override void OnNetworkUpdate() {
			Utility.Send((ProjectilePool_Message_Update) (info, GetPredictedPosition()));
		}

		Vector3 GetPredictedPosition() {
			Vector3 result = (Vector2) position.current + direction * predictionFactor * speed;
			result.z = position.current.z;
			return result;
		}
	}
}