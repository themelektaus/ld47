using UnityEngine;
using E = UnityEngine.Events;
using Mirror;

namespace MT.Packages.LD47
{
	public class Character : NetworkBehaviour, IHostile
	{
		public bool clientAuthority;

		[SerializeField] bool logging = false;

		[SyncVar(hook = nameof(Hook_InGame_Changed))] public bool inGame;
		[SyncVar] public bool isBot;

		[SyncVar] public byte ringIndex;

		[Range(-1, 1)] public float inputHorizontal;
		public bool inputJump;
		public bool inputJumpHold;
		public Transform sweetspot;

		[HideInInspector] public E.UnityEvent<float> onReceiveDamage = new E.UnityEvent<float>();

		[ReadOnly] public Attractor attractor;

		[ResourcePath("prefab"), SyncVar] public string weaponName = null;

		[SerializeField] Audio.SoundEffect hitSoundEffect = null;
		[SerializeField] float maxHealth = 10;
		[SerializeField, SyncVar] float currentHealth;

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
		public SmoothVector3 aimPosition;

		bool jumping;
		float jumpTime;
		float jumpCurveTime;
		float groundTime;

		[ReadOnly] public Weapon weaponInstance;

		public byte GetRingIndex() {
			return ringIndex;
		}

		public byte GetFraction() {
			return 1;
		}

		public new bool hasAuthority { get { return clientAuthority ? base.hasAuthority : isServer; } }
		public bool isDead { get { return currentHealth == 0; } }
		public bool isInGameAndAlive { get { return inGame && !isDead; } }

		void Awake() {
			this.Logging(logging);
			NetworkManager.Register(this);
			attractor = GetComponent<Attractor>();
			aimPosition = new SmoothVector3(transform.position + Vector3.right * 3, .2f);
			Hook_InGame_Changed(false, false);
		}

		void OnDestroy() {
			NetworkManager.Unregister(this);
		}

		void Update() {
			if (hasAuthority) {
				if (isInGameAndAlive) {
					UpdateInput();
					if (appereanceTimer.Update()) {
						ApplyAppearance(
							aimPosition.current,
							attractor.localVelocityX
						);
					}
				} else {
					attractor.localVelocityX = 0;
					aimPosition.value = transform.position + Vector3.right;
				}
			} else {
				aimPosition.Update();
			}
			if (isDead) {
				jumping = false;
				animator.SetBool("Dead", true);
				return;
			}
			animator.SetBool("Dead", false);
			UpdateWeapon();
			UpdateHeadTracking();
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
				var weapon = Resources.Load<Weapon>(weaponName);
				if (!weapon) {
					weaponName = weaponInstance ? weaponInstance.name : "";
					this.LogError("!weapon");
					return;
				}
				weaponInstance = Instantiate(weapon, hand.GetChild(0));
				weaponInstance.name = weaponName;
				weaponInstance.transform.localEulerAngles = new Vector3(0, 0, -90);
				weaponInstance.character = this;
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

		public void AbortJump() {
			jumping = false;
			jumpTime = 0;
			jumpCurveTime = 0;
			groundTime = 0;
		}

		void UpdateHeadTracking() {
			var angle = Utils.GetAngle2D(hand.position, aimPosition.current) - transform.localEulerAngles.z;
			if (angle < -180) {
				angle += 360;
			} else if (angle > 180) {
				angle -= 360;
			}
			if (angle < 0) {
				angle = Mathf.Clamp(angle, -140, -40);
			} else {
				angle = Mathf.Clamp(angle, 40, 140);
			}
			var scale = animator.transform.localScale;
			if (transform.InverseTransformPoint(aimPosition.current).x < 0) {
				scale.x = -1;
				neck.localEulerAngles = new Vector3(0, 0, 90 - angle);
				hand.localEulerAngles = new Vector3(0, 0, 180 - angle);
			} else {
				scale.x = 1;
				neck.localEulerAngles = new Vector3(0, 0, angle + 90);
				hand.localEulerAngles = new Vector3(0, 0, angle + 180);
			}
			animator.transform.localScale = scale;
		}

		void FixedUpdate() {
			if (!hasAuthority && !isServer) {
				return;
			}
			if (jumping) {
				attractor.velocityY = jumpCurve.Evaluate(jumpCurveTime) * jumpForce;
			}
		}

		public void Shoot(Vector2 targetPosition) {
			if (weaponInstance && isInGameAndAlive) {
				weaponInstance.Shoot(GetRingIndex(), GetFraction(), targetPosition);
			}
		}

		public void ReceiveDamage(uint senderID, float damage) {
			if (senderID > 0) {
				Command_TakeDamage(senderID, damage);
				return;
			} else if (clientAuthority) {
				ClientRpc_TakeDamage(senderID, damage);
			} else {
				Server_TakeDamage(senderID, damage);
			}
		}



		[Command(ignoreAuthority = true)]
		void Command_TakeDamage(uint senderID, float damage) {
			this.Log($"[Command(ignoreAuthority = true)] Command_TakeDamage({senderID}, {damage})");
			ClientRpc_TakeDamage(senderID, damage);
		}

		[ClientRpc]
		void ClientRpc_TakeDamage(uint senderID, float damage) {
			this.Log($"[ClientRpc] ClientRpc_TakeDamage({senderID}, {damage})");
			TakeDamageFX();
			onReceiveDamage.Invoke(damage);
			if (Utils.IsMine(senderID)) {
				CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
			}
		}

		[Server]
		void Server_TakeDamage(uint senderID, float damage) {
			Server_AddHealth(-damage);
			ClientRpc_TakeDamage(senderID, damage);
		}

		void TakeDamageFX() {
			var effect = Instantiate(hitEffect).ToTempInstance();
			effect.transform.position = attractor.body.worldCenterOfMass;
			hitSoundEffect.Play(NetworkManager.self, transform.position);
		}



		public void Remote_SetInGame(bool inGame) {
			if (isServer) {
				Server_SetInGame(inGame);
				return;
			}
			Command_SetInGame(inGame);
		}

		[Command]
		public void Command_SetInGame(bool inGame) {
			Server_SetInGame(inGame);
		}

		[Server]
		public void Server_SetInGame(bool inGame) {
			Hook_InGame_Changed(this.inGame, inGame);
			this.inGame = inGame;
		}

		void Hook_InGame_Changed(bool _, bool inGame) {
			animator.gameObject.SetActive(inGame);
			GetComponent<CapsuleCollider2D>().enabled = inGame;
			if (inGame) {
				GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
				attractor.mode = hasAuthority ? Attractor.Mode.Normal : Attractor.Mode.Kinematic;
			} else {
				GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
				attractor.mode = Attractor.Mode.Frozen;
			}
		}



		public void Remote_SetBot(bool isBot) {
			if (isServer) {
				Server_SetBot(isBot);
				return;
			}
			Command_SetBot(isBot);
		}

		[Command]
		public void Command_SetBot(bool isBot) {
			this.Log($"[Command] Command_SetBot({isBot})");
			Server_SetBot(isBot);
		}

		[Server]
		public void Server_SetBot(bool isBot) {
			this.Log($"[Server] Server_SetBot({isBot})");
			this.isBot = isBot;
		}



		public void Remote_Heal() {
			if (isServer) {
				Server_Heal();
				return;
			}
			Command_Heal();
		}

		[Command]
		public void Command_Heal() {
			this.Log($"[Command] Command_Heal()");
			Server_Heal();
		}

		[Server]
		public void Server_Heal() {
			this.Log($"[Server] Server_Heal()");
			currentHealth = maxHealth;
		}



		public void Remote_Kill() {
			if (isServer) {
				Server_Kill();
				return;
			}
			Command_Kill();
		}

		[Command]
		public void Command_Kill() {
			this.Log($"[Command] Command_Kill()");
			Server_Kill();
		}

		[Server]
		public void Server_Kill() {
			this.Log($"[Server] Server_Kill()");
			currentHealth = 0;
		}



		public void Remote_AddHealth(float delta) {
			if (isServer) {
				Server_AddHealth(delta);
				return;
			}
			Command_AddHealth(delta);
		}

		[Command]
		public void Command_AddHealth(float delta) {
			this.Log($"[Command] Command_AddHealth({delta})");
			Server_AddHealth(delta);
		}

		[Server]
		public void Server_AddHealth(float delta) {
			this.Log($"[Server] Server_AddHealth({delta})");
			currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
		}



		public void Remote_SetWeapon(string weaponName) {
			if (isServer) {
				Server_SetWeapon(weaponName);
				return;
			}
			Command_SetWeapon(weaponName);
		}

		[Command]
		public void Command_SetWeapon(string weaponName) {
			this.Log($"[Command] Command_SetWeapon(\"{weaponName}\")");
			Server_SetWeapon(weaponName);
		}

		[Server]
		public void Server_SetWeapon(string weaponName) {
			this.Log($"[Server] Server_SetWeapon(\"{weaponName}\")");
			this.weaponName = weaponName;
		}



		public void Remote_SetRingIndex(byte ringIndex) {
			if (isServer) {
				Server_SetRingIndex(ringIndex);
				return;
			}
			Command_SetRingIndex(ringIndex);
		}

		[Command]
		public void Command_SetRingIndex(byte ringIndex) {
			this.Log($"[Command] Command_SetRingIndex({ringIndex})");
			Server_SetRingIndex(ringIndex);
		}

		[Server]
		public void Server_SetRingIndex(byte ringIndex) {
			this.Log($"[Server] Server_SetRingIndex({ringIndex})");
			this.ringIndex = ringIndex;
		}



		public void ApplyAppearance(Vector3 aimPosition, float localVelocityX) {
			// this.Log("ApplyAppearance(...)");
			if (isServer) {
				ClientRpc_ApplyAppearance(aimPosition, localVelocityX);
				return;
			}
			Command_ApplyAppearance(aimPosition, localVelocityX);
		}

		[Command]
		void Command_ApplyAppearance(Vector3 aimPosition, float localVelocityX) {
			// this.Log($"[Command] Command_ApplyAppearance(...)");
			ClientRpc_ApplyAppearance(aimPosition, localVelocityX);
		}

		[ClientRpc(excludeOwner = true)]
		void ClientRpc_ApplyAppearance(Vector3 aimPosition, float localVelocityX) {
			// this.Log($"[ClientRpc(excludeOwner = true)] ClientRpc_ApplyAppearance(...)");
			this.aimPosition.target = aimPosition;
			attractor.localVelocityX = localVelocityX;
		}



		public bool CanTakeWeapon(string weaponName, int weaponRank) {
			if (!isInGameAndAlive) {
				// this.Log("!isInGameAndAlive ... => false");
				return false;
			}
			if (this.weaponName != weaponName) {
				this.Log("this.weaponName != weaponName ... => true");
				return true;
			}
			if (!weaponInstance) {
				// this.Log("!weaponInstance ... => false");
				return false;
			}
			if (weaponInstance.rank > weaponRank) {
				// this.Log("weaponInstance.rank > weaponRank ... => false");
				return false;
			}
			if (weaponInstance.currentAmmo < weaponInstance.maxAmmo) {
				this.Log("weaponInstance.currentAmmo < weaponInstance.maxAmmo ... => true");
				return true;
			}
			// this.Log($"CanTakeWeapon(\"{weaponName}\", {rank}, {ammo}) ... => false");
			return false;
		}



		void TakeWeapon(string weaponName) {
			if (this.weaponName != weaponName) {
				if (isServer) {
					Server_SetWeapon(weaponName);
				} else {
					Command_SetWeapon(weaponName);
				}
			} else if (weaponInstance && weaponInstance.currentAmmo < weaponInstance.maxAmmo) {
				weaponInstance.FillAmmo();
			}
		}

		[Server]
		public bool Server_TryTakeWeapon(string weaponName, int weaponRank) {
			// this.Log($"[Server] Server_TryTakeWeapon(\",{weaponName}\", {weaponRank})");
			if (CanTakeWeapon(weaponName, weaponRank)) {
				TakeWeapon(weaponName);
				return true;
			}
			return false;
		}

		[TargetRpc]
		public void TargetRpc_TryTakeWeapon(NetworkConnection target, NetworkIdentity server, string weaponName, int weaponRank) {
			this.Log($"[TargetRpc] TargetRpc_TryTakeWeapon({target.identity.netId}, {server.netId}, \",{weaponName}\", {weaponRank})");
			if (CanTakeWeapon(weaponName, weaponRank)) {
				if (server.TryGetComponent(out CollectableWeapon collectableWeapon)) {
					collectableWeapon.Command_Collect();
				}
			}
		}

		[TargetRpc]
		public void TargetRpc_TakeWeapon(NetworkConnection target, string weaponName) {
			this.Log($"[TargetRpc] TargetRpc_TakeWeapon({target.identity.netId}, \",{weaponName}\")");
			TakeWeapon(weaponName);
		}



		public void Respawn(Vector3 position, byte ringIndex) {
			if (!hasAuthority) {
				Debug.LogError("You do not have authority over this object, so you can not respawn :(");
				return;
			}
			if (isDead) {
				Remote_Heal();
				Remote_SetRingIndex(ringIndex);
				Remote_Heal();
				Remote_SetInGame(true);
				if (TryGetComponent<NetworkPosition>(out var networkPosition)) {
					networkPosition.SetPosition(position);
					if (!isServer) {
						networkPosition.Command_SetPositionToServer(position);
					}
					attractor.rotation.value = attractor.GetAngle();
				} else {
					Debug.LogError("There is no NetworkPosition-Component attached to the character");
				}
				return;
			}
			Debug.LogWarning("" +
				"Respawning not possible because you're not dead yet, " +
				"but that's okay... just continue :)"
			);
		}



		void OnDrawGizmos() {
			if (!aimPosition) {
				return;
			}
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(aimPosition.current, Mathf.Abs(Time.time % 2 - 1) * .2f + .2f);
		}
	}
}