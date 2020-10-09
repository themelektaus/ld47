using UnityEngine;

[RequireComponent(typeof(Attractor2D))]
public class BatEnemy2D : MonoBehaviour, IObjective
{
	public enum State
	{
		Idle,
		Attack
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
	Attractor2D affector;
	float projectileTime;
	float alternativeAttackTime;
	Vector2 alternativeAttackDirection;

	float _health;

	void Awake() {
		affector = GetComponent<Attractor2D>();
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
				var playerPosition = Player2D.instance.affector.position;
				if (Vector2.Distance(affector.position, playerPosition) < 2) {
					alternativeAttackTime = .25f;
					alternativeAttackDirection = Utils.GetDirection2D(playerPosition, affector.position);
				}
				if (alternativeAttackTime > 0) {
					alternativeAttackTime -= Time.fixedDeltaTime;
					direction = alternativeAttackDirection;
				} else {
					direction = Utils.GetDirection2D(affector.position, playerPosition);
				}
				affector.kinematicMovement = direction * attackMoveSpeed;
				break;
		}
	}

	public void ReceiveDamage(float damage) {
		AudioManager.instance.PlayHit();
		CameraShake2D.Add(CameraControl.instance.enemyReceiveDamageShake);
		var effect = Instantiate(hitEffect);
		effect.transform.position = affector.position;
		state = State.Attack;
		_health -= damage;
		if (_health <= 0) {
			gameObject.SetActive(false);
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