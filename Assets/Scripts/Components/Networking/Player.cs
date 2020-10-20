using UnityEngine;
using System.Linq;
using E = UnityEngine.Events;
using G = System.Collections.Generic;
using Mirror;

namespace MT.Packages.LD47
{
	public class Player : NetworkBehaviour, IHostile
	{
		public static G.List<Player> players = new G.List<Player>();

		public static Player GetClosestPlayer(Vector2 position) {
			return players
				.Where(x => !x.IsDead() && x.isActiveAndEnabled)
				.Select(player => {
					float distance = Vector2.Distance(position, player.transform.position);
					return (player, distance);
				})
				.OrderBy(x => x.distance)
				.FirstOrDefault()
				.player;
		}

		[Range(-1, 1)] public float inputHorizontal;
		public bool inputJump;
		public bool inputJumpHold;
		public bool flipped;
		public Transform sweetspot;

		[HideInInspector] public E.UnityEvent<float> onReceiveDamage = new E.UnityEvent<float>();

		[ReadOnly] public Attractor attractor;
		[ReadOnly, SyncVar] public float currentHealth;

		[ResourcePath("prefab")] public string weapon = null;
		public Audio.SoundEffect hitSoundEffect = null;
		public float health = 10;

		[SerializeField] GameObject hitEffect = null;
		
		[SerializeField] Animator animator = null;
		[SerializeField] Transform neck = null;
		[SerializeField] Transform hand = null;
		
		[SerializeField] float moveSpeed = 13;
		[SerializeField] float jumpForce = 4.2f;
		[SerializeField] AnimationCurve jumpCurve = new AnimationCurve();
		[SerializeField] float jumpCurveDuration = .3f;
		[SerializeField, Range(0, 1)] float airJumpForgiveness = .4f;

		[SyncVar] public Vector3 trackingPosition;

		bool jumping;
		float jumpTime;
		float jumpCurveTime;
		float groundTime;

		[ReadOnly] public Weapon weaponInstance;

		void Awake() {
			attractor = GetComponent<Attractor>();
		}

		public override void OnStartServer() {
			base.OnStartServer();
			players.Add(this);
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (isLocalPlayer) {
				attractor.state = Attractor.State.Normal;
				trackingPosition = transform.position + Vector3.right * 3;
				currentHealth = health;
				gameObject.AddComponent<PlayerController>();
				if (Camera.main.TryGetComponent<SimpleInterpolation>(out var interpolation)) {
					interpolation.target = gameObject.transform;
				}
			}
		}

		public void SetDead() {
			if (IsDead()) {
				return;
			}
			animator.SetBool("Dead", true);
		}

		void OnDestroy() {
			players.Remove(this);
		}

		void Update() {
			if (IsDead()) {
				attractor.localVelocityX = 0;
				trackingPosition = transform.position + Vector3.right;
			} else {
				UpdateWeapon();
				UpdateInput();
				UpdateFlip();
				UpdateBodyTracking();
			}
		}

		void UpdateWeapon() {
			if (string.IsNullOrEmpty(weapon) && weaponInstance) {
				Destroy(weaponInstance.gameObject);
				weaponInstance = null;
			} else if ((!string.IsNullOrEmpty(weapon) && !weaponInstance) || (weaponInstance && weapon != weaponInstance.name)) {
				if (weaponInstance) {
					Destroy(weaponInstance.gameObject);
					weaponInstance = null;
				}
				weaponInstance = Instantiate(Resources.Load<Weapon>(weapon), hand.GetChild(0));
				weaponInstance.name = weapon;
				weaponInstance.transform.localEulerAngles = new Vector3(0, 0, -90);
				weaponInstance.player = this;
			}
		}

		void UpdateInput() {
			attractor.localVelocityX = inputHorizontal * moveSpeed;
			if (jumping) {
				if (inputJumpHold) {
					jumpCurveTime += Time.deltaTime / jumpCurveDuration;
					if (jumpCurveTime >= 1) {
						jumpCurveTime = 1;
						jumping = false;
					}
				} else {
					jumping = false;
				}
			} else {
				groundTime = attractor.grounded ? airJumpForgiveness : Mathf.Max(0, groundTime - Time.deltaTime);
			}
			if (inputJump && jumpTime == 0 && groundTime > 0) {
				jumping = true;
				jumpTime = .15f;
				jumpCurveTime = 0;
				groundTime = 0;
				attractor.ResetVelocityY();
			} else {
				jumpTime = Mathf.Max(0, jumpTime - Time.deltaTime);
			}
		}

		void UpdateFlip() {
			var scale = animator.transform.localScale;
			scale.x = flipped ? -1 : 1;
			animator.transform.localScale = scale;
		}

		void UpdateBodyTracking() {
			var angle = Utils.GetAngle2D(hand.position, trackingPosition);
			if (animator.transform.localScale.x < 0) {
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

		void FixedUpdate() {
			if (!isLocalPlayer) {
				return;
			}
			if (jumping) {
				attractor.velocityY = jumpCurve.Evaluate(jumpCurveTime) * jumpForce;
			}
		}

		public bool IsDead() {
			return animator.GetBool("Dead");
		}

		public void ReceiveDamage(string senderTag, float damage) {
			if (damage == 0) {
				return;
			}
			// this.Send(nameof(RFC_TakeDamage), Target.All, damage);
			// if (CompareTag(senderTag) && tno.ownerID != senderID) {
			// 	this.Send(nameof(RFC_TakeDamageSound), senderID);
			// }
		}

		public void Shoot(Vector2 targetPosition) {
			if (!IsDead() && weaponInstance) {
				weaponInstance.Shoot(tag, targetPosition);
			}
		}

		// [RFC]
		// void RFC_UpdateRemoteData(RemoteData data) {
		// 	transformPosition.target = data.targetPosition;
		// 	trackingPosition.target = data.targetTrackingPosition;
		// 	flipped = data.flipped;
		// 	inputHorizontal = data.inputHorizontal;
		// 	weapon = data.weapon;
		// 	if (data.dead) {
		// 		SetDead();
		// 	}
		// }

		// [RFC]
		// void RFC_TakeDamage(float damage) {
		// 	var effect = Instantiate(hitEffect).ToTempInstance();
		// 	effect.transform.position = attractor.body.worldCenterOfMass;
		// 	onReceiveDamage.Invoke(damage);
		// }

		// [RFC]
		// void RFC_TakeDamageSound() {
		// 	hitSoundEffect.Play(this);
		// 	CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		// }
	}
}