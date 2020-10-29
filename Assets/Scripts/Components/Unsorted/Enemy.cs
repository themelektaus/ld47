using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public abstract class Enemy : NetworkBehaviour, IHostile
	{
		public enum State
		{
			Idle,
			Attack
		}

		protected Vector2 startPosition { get; private set; }

		[SerializeField] float maxHealth = 3;
		[SerializeField] protected State state = State.Idle;

		[SerializeField] GameObject hitEffect = null;
		[SerializeField] Audio.SoundEffect hitSoundEffect = null;

		protected Animator animator;
		float currentHealth;
		SpriteRenderer[] renderers;

		readonly SmoothFloat alpha = (1, .5f);

		[SyncVar] public bool ready;
		[SyncVar] public bool dead;
		[SyncVar] public byte ringIndex;

		public bool isReadyAndAlive { get { return ready && !dead; } }

		public byte GetRingIndex() {
			return ringIndex;
		}

		public byte GetFraction() {
			return 2;
		}

		protected virtual void Awake() {
			NetworkManager.Register(this);
			animator = GetComponentInChildren<Animator>();
			renderers = GetComponentsInChildren<SpriteRenderer>();
			foreach (var renderer in renderers) {
				renderer.enabled = false;
			}
		}

		void OnDestroy() {
			NetworkManager.Unregister(this);
		}

		public override void OnStartServer() {
			base.OnStartServer();
			currentHealth = maxHealth;
			startPosition = transform.position;
		}

		[ServerCallback]
		protected void Update() {
			animator.SetBool("Dead", dead);
			OnServerUpdate();
		}

		protected abstract void OnServerUpdate();

		[ClientCallback]
		void LateUpdate() {
			if (CharacterController.exists && CharacterController.instance.character.GetRingIndex() != GetRingIndex()) {
				alpha.target = .3f;
			} else {
				alpha.target = 1;
			}
			alpha.Update();
			foreach (var renderer in renderers) {
				var color = renderer.color;
				color.a *= alpha.current;
				renderer.color = color;
			}
		}

		public void ReceiveDamage(uint senderID, float damage) {
			if (isServer) {
				TakeDamageData(damage);
				TakeDamageFX();
				RPC_TakeDamageFX(0);
				if (Utils.TryGetConnection(senderID, out var connection)) {
					TargetRpc_CameraShake(connection);
				}
				return;
			}
			TakeDamage(senderID, damage);
			TakeDamageFX();
			CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		}

		[TargetRpc]
		void TargetRpc_CameraShake(NetworkConnection target) {
			this.Log("[TargetRpc] TargetRPC_CameraShake(...)");
			CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
		}

		[Command(ignoreAuthority = true)]
		void TakeDamage(uint senderID, float damage) {
			TakeDamageData(damage);
			RPC_TakeDamageFX(senderID);
		}

		void TakeDamageData(float damage) {
			state = State.Attack;
			currentHealth -= damage;
			if (currentHealth <= 0) {
				dead = true;
			}
		}

		void TakeDamageFX() {
			hitSoundEffect.Play(NetworkManager.self, transform.position);
			Instantiate(hitEffect).ToTempInstance().transform.position = transform.position;
		}

		[ClientRpc]
		void RPC_TakeDamageFX(uint senderID) {
			if (!Utils.IsMine(senderID)) {
				TakeDamageFX();
			}
		}
	}
}