using UnityEngine;

[RequireComponent(typeof(Affector2D))]
public class Player2D : MonoBehaviour, IObjective
{
	public static Player2D instance;

	public float health = 5;
	public float moveSpeed = 13;
	public float jumpForce = 4;
	public AnimationCurve jumpCurve;
	public float jumpCurveDuration = .3f;
	public int ringIndex;

	public Transform sprites;
	public Transform neck;
	public Transform hand;
	public Projectile weaponProjectile;
	public Transform wepaonFront;
	public AudioClip wep_AO_nShootClip;
	// public Weapon weapon;

	[System.NonSerialized] public Affector2D affector;

	bool jumping = true;
	float jumpTime;
	float jumpCurveTime;
	Vector2 startPosition;
	float _health;

	void Awake() {
		instance = this;
		affector = GetComponent<Affector2D>();
		startPosition = affector.position;
		_health = health;
	}

	void FixedUpdate() {
		if (jumping) {
			affector.SetYVelocity(jumpCurve.Evaluate(jumpCurveTime) * jumpForce);
		}
	}

	void Update() {
		if (_health <= 0) {
			ResetGame();
			return;
		}

		UpdateMovement();
		UpdateJumping();
		UpdateFlip();
		UpdateBodyTracking();
		UpdateInput();
	}

	void ResetGame() {
		foreach (var changeRing in FindObjectsOfType<ChangeRing>(true)) {
			changeRing.activated = false;
		}
		foreach (var enemy in FindObjectsOfType<BatEnemy2D>(true)) {
			enemy.OnResetGame();
		}
		foreach (var changeRing in FindObjectsOfType<ChangeRing>()) {
			changeRing.particleEffect.SetActive(false);
		}
		affector.velocity = Vector2.zero;
		affector.position = startPosition;
		ringIndex = 0;
		_health = health;
	}

	void UpdateFlip() {
		Vector2 affectorPosition = Camera.main.WorldToScreenPoint(affector.position);
		var scale = sprites.localScale;
		scale.x = Input.mousePosition.x < affectorPosition.x ? -1 : 1;
		sprites.localScale = scale;
	}

	void UpdateBodyTracking() {
		var angle = Utils.GetAngle2D(hand.position, GetMouseWorldPosition());
		if (sprites.localScale.x < 0) {
			neck.eulerAngles = new Vector3(0, 0, angle - 90);
			hand.eulerAngles = new Vector3(0, 0, angle - 180);
		} else {
			neck.eulerAngles = new Vector3(0, 0, angle + 90);
			hand.eulerAngles = new Vector3(0, 0, angle + 180);
		}
		var eulerAngles = neck.localEulerAngles;
		if (eulerAngles.z > 40 && eulerAngles.z < 270) {
			eulerAngles.z = 40;
		}
		neck.localEulerAngles = eulerAngles;
	}

	void UpdateMovement() {
		affector.localVelocityX = Input.GetAxis("Horizontal") * moveSpeed;
	}

	void UpdateJumping() {
		if (!jumping) {
			return;
		}
		if (Input.GetButton("Jump")) {
			jumpCurveTime += Time.deltaTime / jumpCurveDuration;
			if (jumpCurveTime >= 1) {
				jumpCurveTime = 1;
				jumping = false;
			}
		} else {
			jumping = false;
		}
	}

	void UpdateInput() {
		if (Input.GetButtonDown("Jump") && jumpTime == 0 && affector.grounded) {
			Jump();
		} else {
			jumpTime = Mathf.Max(0, jumpTime - Time.deltaTime);
		}
		if (Input.GetMouseButtonDown(0)) {
			AudioManager.instance.Play(wep_AO_nShootClip, .6f, 1);
			Projectile.Spawn(affector, wepaonFront.position, weaponProjectile, transform, GetMouseWorldPosition());
		}
	}

	Vector2 GetMouseWorldPosition() {
		return Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

	void Jump() {
		jumping = true;
		jumpTime = .15f;
		jumpCurveTime = 0;
	}

	public void ReceiveDamage(float damage) {
		_health -= 1;
		EZCameraShake.CameraShaker.Instance.ShakeOnce(.7f, 6.1f, .1f, 1f);
		AudioManager.instance.PlayPlayerHit();
	}

	public int GetRingIndex() {
		return ringIndex;
	}

	public float GetHealthPercent() {
		return _health / health;
	}
}