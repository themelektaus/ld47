using UnityEngine;
using E = UnityEngine.Events;
using Mirror;
using MT.Packages.Core;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(NetworkObjectInfo))]
	public class Character : NetworkBehaviour, IHostile
	{
		public bool clientAuthority;

		[SyncVar(hook = nameof(Hook_InGame_Changed))] public bool inGame;
		[SyncVar] public bool isBot;

		[Range(-1, 1)] public float inputHorizontal;
		public bool inputJump;
		public bool inputJumpHold;
		public Transform sweetspot;

		[HideInInspector] public E.UnityEvent<float> onReceiveDamage = new E.UnityEvent<float>();

		[Core.Attributes.ReadOnly] public Attractor attractor;

		[Core.Attributes.ResourcePath("prefab"), SyncVar] public string weaponName = null;

		[SerializeField] AudioSystem.SoundEffect hitSoundEffect = null;

		[SerializeField] float maxHealth = 10;
		[SerializeField, SyncVar] float currentHealth = 0;

		[SerializeField] GameObject hitEffect = null;

		public Animator animator;

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

		[Core.Attributes.ReadOnly] public Weapon weaponInstance;

		public float GetHealthPercentage() {
			return currentHealth / maxHealth;
		}

		public float GetAmmoPercentage() {
			if (!weaponInstance) {
				return 0;
			}
			return weaponInstance.currentAmmo / weaponInstance.maxAmmo;
		}

		public float GetCastedTimePercentage() {
			if (!weaponInstance) {
				return 0;
			}
			if (weaponInstance.castTime == 0) {
				return 0;
			}
			return 1 - weaponInstance.castedTime / weaponInstance.castTime;
		}

		public byte GetFraction() {
			return 1;
		}

		public new bool hasAuthority { get { return clientAuthority ? base.hasAuthority : isServer; } }

		public bool isDead { get { return currentHealth == 0; } }

		public bool isInGameAndDead { get { return inGame && isDead; } }

		public bool isInGameAndAlive { get { return inGame && !isDead; } }

		void Awake() {
			NetworkManager.Register(this);
			attractor = GetComponent<Attractor>();
			aimPosition = new SmoothVector3(transform.position + Vector3.right * 3, .2f);
			Hook_InGame_Changed(false, false);
		}

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();
			if (animator.TryGetComponent(out Saveable saveable)) {
				saveable.Load();
			}
			if (animator.TryGetComponent(out CharacterSkin skin)) {
				Command_ApplySkin(skin.data);
			}
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
				var child = hand.GetChild(0);
				weaponInstance = Instantiate(weapon, child);
				weaponInstance.name = weaponName;
				var scale = weaponInstance.transform.localScale;
				scale.x /= child.localScale.x * .9f;
				scale.y /= child.localScale.y * .9f;
				weaponInstance.transform.localScale = scale;
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
			var handAngle = Utility.GetAngle2D(hand.position, aimPosition.current) - transform.localEulerAngles.z;
			if (handAngle < -180) {
				handAngle += 360;
			} else if (handAngle > 180) {
				handAngle -= 360;
			}
			float neckAngle;
			if (handAngle < 0) {
				neckAngle = Mathf.Clamp(handAngle, -140, -40);
			} else {
				neckAngle = Mathf.Clamp(handAngle, 40, 140);
			}
			var scale = animator.transform.localScale;
			if (transform.InverseTransformPoint(aimPosition.current).x < 0) {
				scale.x = -1;
				neck.localEulerAngles = new Vector3(0, 0, 90 - neckAngle);
				hand.localEulerAngles = new Vector3(0, 0, 180 - handAngle);
			} else {
				scale.x = 1;
				neck.localEulerAngles = new Vector3(0, 0, neckAngle + 90);
				hand.localEulerAngles = new Vector3(0, 0, handAngle + 180);
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
				weaponInstance.Shoot(this.GetRingIndex(), GetFraction(), targetPosition);
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
			// this.Log($"[Command(ignoreAuthority = true)] Command_TakeDamage({senderID}, {damage})");
			ClientRpc_TakeDamage(senderID, damage);
		}

		[ClientRpc]
		void ClientRpc_TakeDamage(uint senderID, float damage) {
			// this.Log($"[ClientRpc] ClientRpc_TakeDamage({senderID}, {damage})");
			TakeDamageFX();
			onReceiveDamage.Invoke(damage);
		}

		[Server]
		void Server_TakeDamage(uint senderID, float damage) {
			Server_AddHealth(-damage);
			ClientRpc_TakeDamage(senderID, damage);
		}

		void TakeDamageFX() {
			var effect = Instantiate(hitEffect).ToTempInstance();
			effect.transform.position = attractor.body.worldCenterOfMass;
			hitSoundEffect.Play(transform.position);
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
			this.SetRingIndex(ringIndex);
		}



		public void Remote_Instantiate(string resourcePath, Vector3 position) {
			Instantiate(resourcePath, position);
			if (isServer) {
				ClientRpc_Instantiate(resourcePath, position);
			} else {
				Command_Instantiate(resourcePath, position);
			}
		}

		[Command]
		void Command_Instantiate(string resourcePath, Vector3 position) {
			ClientRpc_Instantiate(resourcePath, position);
		}

		[ClientRpc(excludeOwner = true)]
		void ClientRpc_Instantiate(string resourcePath, Vector3 position) {
			Instantiate(resourcePath, position);
		}

		void Instantiate(string resourcePath, Vector3 position) {
			Instantiate(Core.Cache.Get(resourcePath), position, Quaternion.identity).ToTempInstance();
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



		void ApplySkin(CharacterSkin.Data data) {
			if (animator.TryGetComponent(out CharacterSkin skin)) {
				skin.data = data;
			}
		}

		[Command]
		public void Command_ApplySkin(CharacterSkin.Data data) {
			this.Log($"[Command] Command_ApplySkin({data})");
			ApplySkin(data);
			ClientRpc_ApplySkin(data);
			NetworkManager.instance.Server_SendCharacterSkins();
		}

		[ClientRpc(excludeOwner = true)]
		public void ClientRpc_ApplySkin(CharacterSkin.Data data) {
			this.Log($"[ClientRpc(excludeOwner = true)] ClientRpc_ApplySkin({data})");
			ApplySkin(data);
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