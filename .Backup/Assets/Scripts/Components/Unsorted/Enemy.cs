using Mirror;
using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(NetworkObjectInfo))]
	public abstract class Enemy : NetworkBehaviour, IHostile
	{
		public enum State
		{
			Idle,
			Attack,
			Ignore
		}

		protected abstract bool ringIndexFading { get; }

		protected Vector2 startPosition { get; private set; }

		[SerializeField] float maxHealth = 3;
		[SerializeField] public State state = State.Idle;

		[SerializeField] GameObject hitEffect = null;
		[SerializeField] float hitEffectScale = 1;
		[SerializeField] AudioSystem.SoundEffect hitSoundEffect = null;

		protected Animator animator;
		float currentHealth;
		SpriteRenderer[] renderers;

		readonly SmoothFloat alpha = (1, .5f);

		[SyncVar] public bool ready;
		[SyncVar] public bool dead;

		public bool isReadyAndAlive { get { return ready && !dead; } }

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

		public override void OnStartClient() {
			base.OnStartClient();
			if (isServer) {
				return;
			}
			if (TryGetComponent(out Attractor attractor)) {
				attractor.mode = Attractor.Mode.Frozen;
				attractor.body.bodyType = RigidbodyType2D.Kinematic;
			}
		}

		[ServerCallback]
		protected void Update() {
			animator.SetBool("Dead", dead);
			OnServerUpdate();
		}

		protected abstract void OnServerUpdate();
		protected abstract void OnFixedUpdate_Idle();
		protected abstract void OnFixedUpdate_Attack();

		[ServerCallback]
		void FixedUpdate() {
			if (!isReadyAndAlive) {
				return;
			}
			switch (state) {
				case State.Idle:
				case State.Ignore:
					OnFixedUpdate_Idle();
					break;
				case State.Attack:
					OnFixedUpdate_Attack();
					break;
			}
		}

		[ClientCallback]
		void LateUpdate() {
			if (!ringIndexFading) {
				return;
			}
			if (CharacterController.instance && CharacterController.instance.character.GetRingIndex() != this.GetRingIndex()) {
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
				return;
			}
			TakeDamage(senderID, damage);
			TakeDamageFX();
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
			hitSoundEffect.Play(NetworkManager.instance, transform.position);
			Instantiate(hitEffect)
				.ToTempInstance()
				.SetTransformPosition(transform.position)
				.SetTransformLocalScale(Vector3.Scale(transform.localScale, hitEffect.transform.localScale * hitEffectScale));
		}

		[ClientRpc]
		void RPC_TakeDamageFX(uint senderID) {
			if (!Utility.IsMine(senderID)) {
				TakeDamageFX();
			}
		}
	}
}