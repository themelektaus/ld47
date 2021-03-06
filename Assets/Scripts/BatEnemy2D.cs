﻿using UnityEngine;

[RequireComponent(typeof(Affector2D))]
public class BatEnemy2D : MonoBehaviour, IObjective
{
	public enum State
	{
		Idle,
		Attack,
		AfterMadeDamage
	}

	public int ringIndex;

	public float health = 3;
	public float aggroRadius = 10;
	public float homeRadius = 2;

	public float idleMoveSpeed = .5f;
	public float attackMoveSpeed = 7.5f;
	public float afterMadeDamageMoveSpeed = 4.5f;

	public Projectile projectile;
	public float projectileInterval = 0.5f;
	public GameObject hitEffect;

	[SerializeField] State state = State.Idle;

	Vector2 startPosition;
	Vector2 randomPosition;
	float stateTime;
	Affector2D affector;
	float projectileTime;

	float _health;

	void Awake() {
		affector = GetComponent<Affector2D>();
		startPosition = affector.position;
		randomPosition = GetRandomPosition();
		_health = health;
	}

	public void OnResetGame() {
		_health = health;
		randomPosition = GetRandomPosition();
		affector.position = startPosition;
		SwitchToIdle();
		gameObject.SetActive(true);
	}

	public void SwitchToIdle() {
		if (state != State.Idle) {
			state = State.Idle;
			randomPosition = GetRandomPosition();
		}
	}

	Vector2 GetRandomPosition() {
		return startPosition + Random.insideUnitCircle * homeRadius;
	}

	void Update() {
		if (!Player2D.instance.isActiveAndEnabled || Player2D.instance.ringIndex != ringIndex) {
			SwitchToIdle();
			return;
		}
		if (state == State.AfterMadeDamage) {
			stateTime -= Time.deltaTime;
			if (stateTime <= 0) {
				state = State.Attack;
			}
			return;
		}
		if (state == State.Attack) {
			if (projectile) {
				if (projectileTime > 0) {
					projectileTime -= Time.deltaTime;
				} else {
					Projectile.Spawn(affector, affector.position, projectile, transform, Player2D.instance.affector.position);
					projectileTime += projectileInterval;
				}
			}
			return;
		}
		if (Vector2.Distance(affector.position, Player2D.instance.affector.position) <= aggroRadius) {
			state = State.Attack;
		}
	}

	void FixedUpdate() {
		Vector2 direction;
		switch (state) {
			case State.Idle:
				if (Vector2.Distance(affector.position, randomPosition) > .2f) {
					affector.kinematicMovement = Utils.GetDirection2D(affector.position, randomPosition) * idleMoveSpeed;
				} else {
					randomPosition = GetRandomPosition();
				}
				break;
			case State.Attack:
				direction = Utils.GetDirection2D(affector.position, Player2D.instance.affector.position);
				affector.kinematicMovement = direction * attackMoveSpeed;
				break;
			case State.AfterMadeDamage:
				direction = Utils.GetDirection2D(Player2D.instance.affector.position, affector.position);
				affector.kinematicMovement = direction * afterMadeDamageMoveSpeed;
				break;
		}
	}

	public void ReceiveDamage(float damage) {
		AudioManager.instance.PlayHit();
		var effect = Instantiate(hitEffect);
		effect.transform.position = affector.position;
		state = State.Attack;
		_health -= damage;
		if (_health <= 0) {
			gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (state != State.Attack) {
			return;
		}
		if (collision.TryGetComponent<IObjective>(out var collisionObjective)) {
			if (!collision.CompareTag(tag)) {
				collisionObjective.ReceiveDamage(1);
				state = State.AfterMadeDamage;
				stateTime = .5f;
}
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, aggroRadius);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, homeRadius);
	}

	public int GetRingIndex() {
		return ringIndex;
	}
}