﻿using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Object : Pool_Object
	{
		public SmoothTransformPosition position;
		public Vector2 direction;

		[SerializeField, Range(0, 10)] float damage = 1;
		[SerializeField, Range(1, 50)] float speed = 20;
		[SerializeField] GameObject sprites = null;
		[SerializeField] ParticleSystem[] trailEffects = null;
		[SerializeField, Range(0, 1)] float predictionFactor = .15f;
		[SerializeField] DefaultPool_Object explosion = null;

		void Awake() {
			position = (transform, predictionFactor);
		}

		public override void OnEnableAll() {
			sprites.SetActive(true);
			foreach (var trailEffect in trailEffects) {
				trailEffect.Play();
			}
		}

		public override void OnDisableAll() {
			sprites.SetActive(false);
			foreach (var trailEffect in trailEffects) {
				trailEffect.Stop();
			}
		}

		public override void OnUpdate() {
			transform.eulerAngles = new Vector3(0, 0, Utils.GetAngle2D(direction));
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

		protected override void OnTrigger(Collider2D collision) {
			base.OnTrigger(collision);
			if (collision.HasComponent<ProjectilePool_Object>()) {
				return;
			}
			if (collision.TryGetComponent<IHostile>(out var hostile)) {
				if (info.ownerRingIndex != hostile.GetRingIndex()) {
					return;
				}
				if (info.ownerFraction == hostile.GetFraction()) {
					return;
				}
				if (hostile is Character character && character.isInGameAndAlive) {
					if (damage > 0) {
						character.ReceiveDamage(info.ownerID, damage);
					}
				} else if (hostile is Enemy Enemy && Enemy.isReadyAndAlive) {
					if (damage > 0) {
						Enemy.ReceiveDamage(info.ownerID, damage);
					}
				} else {
					return;
				}
			}
			if (explosion) {
				Pool.Get<DefaultPool>(explosion).Spawn(info.ownerRingIndex, GetPredictedPosition());
			}
			UnSpawn();
		}

		public override void OnNetworkAdd() {
			Utils.Send((ProjectilePool_Message_Add) (info, position.current, direction));
		}

		public override void OnNetworkUpdate() {
			Utils.Send((ProjectilePool_Message_Update) (info, GetPredictedPosition()));
		}

		Vector3 GetPredictedPosition() {
			return (Vector2) position.current + direction * predictionFactor * speed;
		}
	}
}