using UnityEngine;
using E = UnityEngine.Events;
using Mirror;

namespace MT.Packages.LD47
{
	public class Player : NetworkBehaviour, IHostile
	{
		[SerializeField] bool logging = false;

		[ReadOnly, SyncVar(hook = nameof(SetInGame))] bool inGame = true;
		
		[Range(-1, 1)] public float inputHorizontal;
		public bool inputJump;
		public bool inputJumpHold;
		public Transform sweetspot;

		[HideInInspector] public E.UnityEvent<float> onReceiveDamage = new E.UnityEvent<float>();

		[ReadOnly] public Attractor attractor;

		[ResourcePath("prefab"), SyncVar] public string weaponName = null;

		[SerializeField] Audio.SoundEffect hitSoundEffect = null;
		[SerializeField] float maxHealth = 10;
		[SerializeField, ReadOnly, SyncVar] float currentHealth;

		[SerializeField] GameObject hitEffect = null;

		[SerializeField] Animator animator = null;
		[SerializeField] Transform neck = null;
		[SerializeField] Transform hand = null;

		[SerializeField] float moveSpeed = 13;
		[SerializeField] float jumpForce = 4.2f;
		[SerializeField] AnimationCurve jumpCurve = new AnimationCurve();
		[SerializeField] float jumpCurveDuration = .3f;
		[SerializeField, Range(0, 1)] float airJumpForgiveness = .4f;

		readonly Timer appereanceTimer = .2f;
		public SmoothVector3 trackingPosition;
		[SyncVar] public bool flipped;

		bool jumping;
		float jumpTime;
		float jumpCurveTime;
		float groundTime;

		[ReadOnly] public Weapon weaponInstance;

		public bool isDead { get { return currentHealth == 0; } }
		public bool isInGameAndAlive { get { return inGame && !isDead; } }

		void Awake() {
			this.Logging(logging);
			attractor = GetComponent<Attractor>();
			trackingPosition = new SmoothVector3(transform.position + Vector3.right * 3, .2f);
		}

		public override void OnStartServer() {
			base.OnStartServer();
			SetInGame(inGame, false);
			NetworkManager.RegisterPlayer(this);
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (isLocalPlayer) {
				gameObject.AddComponent<PlayerController>();
			}
		}

		[ServerCallback]
		void OnDestroy() {
			NetworkManager.UnregisterPlayer(this);
		}

		[Command]
		public void Command_SetInGame(bool inGame) {
			SetInGame(this.inGame, inGame);
		}

		void SetInGame(bool _, bool newValue) {
			inGame = newValue;
			animator.gameObject.SetActive(newValue);
			GetComponent<CapsuleCollider2D>().enabled = newValue;
			if (newValue) {
				GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
				attractor.mode = isLocalPlayer ? Attractor.Mode.Normal : Attractor.Mode.Kinematic;
			} else {
				GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
				attractor.mode = Attractor.Mode.Frozen;
			}
		}

		void Update() {
			if (isLocalPlayer) {
				if (isInGameAndAlive) {
					UpdateInput();
					if (appereanceTimer.Update()) {
						SetAppearance(
							trackingPosition.current,
							flipped,
							animator.GetFloat("Move") != 0,
							weaponInstance ? weaponInstance.currentAmmo : 0
						);
					}
				} else {
					attractor.localVelocityX = 0;
					trackingPosition.value = transform.position + Vector3.right;
				}
			} else {
				trackingPosition.Update();
			}
			if (isDead) {
				jumping = false;
				animator.SetBool("Dead", true);
				return;
			}
			animator.SetBool("Dead", false);
			UpdateWeapon();
			UpdateFlip();
			UpdateBodyTracking();
		}

		void UpdateWeapon() {
			if (string.IsNullOrEmpty(weaponName) && weaponInstance) {
				Destroy(weaponInstance.gameObject);
				weaponInstance = null;
			} else if (!string.IsNullOrEmpty(weaponName) && (!weaponInstance || (weaponName != weaponInstance.name))) {
				if (weaponInstance) {
					Destroy(weaponInstance.gameObject);
					weaponInstance = null;
				}
				weaponInstance = Instantiate(Resources.Load<Weapon>(weaponName), hand.GetChild(0));
				weaponInstance.name = weaponName;
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
			var angle = Utils.GetAngle2D(hand.position, trackingPosition.current);
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

		[ClientCallback]
		void FixedUpdate() {
			if (!isLocalPlayer) {
				return;
			}
			if (jumping) {
				attractor.velocityY = jumpCurve.Evaluate(jumpCurveTime) * jumpForce;
			}
		}

		public void Shoot(Vector2 targetPosition) {
			if (weaponInstance && isInGameAndAlive) {
				weaponInstance.Shoot(targetPosition);
			}
		}

		public void UpdateBotShoot(Vector2 targetPosition) {
			if (weaponInstance && isInGameAndAlive) {
				weaponInstance.UpdateBotShoot(targetPosition);
			}
		}

		public void ReceiveDamage(uint senderID, float damage) {
			if (senderID > 0) {
				Command_TakeDamage(senderID, damage);
				return;
			}
			RPC_TakeDamage(senderID, damage);
		}

		[Command(ignoreAuthority = true)]
		void Command_TakeDamage(uint senderID, float damage) {
			this.Log($"[Command(ignoreAuthority = true)] Command_TakeDamage({senderID}, {damage})");
			RPC_TakeDamage(senderID, damage);
		}

		[ClientRpc]
		void RPC_TakeDamage(uint senderID, float damage) {
			this.Log($"[ClientRpc] RPC_TakeDamage({senderID}, {damage})");
			TakeDamage(senderID, damage);
		}

		void TakeDamage(uint senderID, float damage) {
			var effect = Instantiate(hitEffect).ToTempInstance();
			effect.transform.position = attractor.body.worldCenterOfMass;
			onReceiveDamage.Invoke(damage);
			hitSoundEffect.Play(this);
			if (Utils.IsMine(senderID)) {
				CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
			}
		}

		[Command]
		public void Command_Heal() {
			this.Log($"[Command] Command_Heal()");
			Heal();
		}

		public void Heal() {
			currentHealth = maxHealth;
		}

		[Command]
		public void Kill() {
			this.Log($"[Command] Kill()");
			currentHealth = 0;
		}

		[Command]
		public void ChangeHealth(float delta) {
			this.Log($"[Command] ChangeHealth({delta})");
			currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
		}

		[Command]
		public void SetWeapon(string weaponName) {
			this.Log($"[Command] SetWeapon(\"{weaponName}\")");
			this.weaponName = weaponName;
		}

		[Command]
		void SetAppearance(Vector3 trackingPosition, bool flipped, bool moving, float currentAmmo) {
			this.flipped = flipped;
			if (weaponInstance) {
				weaponInstance.currentAmmo = currentAmmo;
			}
			RPC_GetAppearance(trackingPosition, moving);
		}

		[ClientRpc(excludeOwner = true)]
		void RPC_GetAppearance(Vector3 trackingPosition, bool moving) {
			// this.Log($"[ClientRpc(excludeOwner = true)] RPC_GetAppearance(...)");
			this.trackingPosition.target = trackingPosition;
			attractor.localVelocityX = moving ? 1 : 0;
		}

		[Server]
		public bool TryTakeWeapon(string weaponName, int weaponRank, float weaponAmmo) {
			this.Log($"[Server] TakeWeapon(\"{weaponName}\", {weaponRank}, {weaponAmmo})");
			if (!isInGameAndAlive) {
				this.Log("!isInGameAndAlive ... => false");
				return false;
			}
			if (this.weaponName != weaponName) {
				this.Log("this.weaponName != weaponName ... => true");
				this.weaponName = weaponName;
				return true;
			}
			if (!weaponInstance) {
				this.Log("!weaponInstance ... => false");
				return false;
			}
			if (weaponInstance.rank > weaponRank) {
				this.Log("weaponInstance.rank > weaponRank ... => false");
				return false;
			}
			if (weaponInstance.currentAmmo < weaponAmmo) {
				this.Log("weaponInstance.currentAmmo < weaponAmmo ... => true");
				TargetRPC_FillAmmo();
				return true;
			}
			this.Log($"[Server] TakeWeapon(...) => false");
			return false;
		}

		[TargetRpc]
		void TargetRPC_FillAmmo() {
			this.Log($"[TargetRpc] TargetRPC_FillAmmo()");
			weaponInstance.FillAmmo();
		}

		[Client]
		public void Respawn() {
			if (isDead) {
				Heal();
				Command_Heal();
				Command_SetInGame(true);
			} else {
				Debug.LogError("Respawning not possible because you're not dead yet :)");
			}
		}
	}
}