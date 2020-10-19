using TNet;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Player))]
	public class PlayerController : MonoBehaviour
	{
		[HideInInspector] public Player player;

#if UNITY_EDITOR
		public bool bot = false;
#else
		public bool bot = true;
#endif

		float botHorizontal;
		float botJump;
		readonly Timer botInputHorizontalTimer = new Timer(.5f);
		readonly Timer botInputJumpTimer = new Timer(3);
		readonly Timer botInputShootTimer = new Timer(.35f, .68f);
		readonly Timer botSelectEnemyTimer = new Timer(1);
		BatEnemy botCurrentEnemy;

		void Awake() {
			player = GetComponent<Player>();
			player.attractor.body.mass = 1;
			player.onReceiveDamage.AddListener(damage => {
				player.currentHealth -= damage;
				if (player.currentHealth <= 0) {
					player.SetDead();
					gameObject.DestroySelf(3);
				}
				player.hitSoundEffect.Play(this);
				CameraShake.Add(CameraControl.instance.playerReceiveDamageShake);
			});
		}

		void OnEnable() {
			var texture = Resources.Load<Texture2D>("Aim Cursor");
			Cursor.SetCursor(texture, new Vector2(16, 16), CursorMode.ForceSoftware);
		}

		void OnDisable() {
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}

		void OnDestroy() {
			var button = Button.SetActiveByTag("Spawn Button");
			if (button) {
				button.GetComponentInChildren<UnityEngine.UI.Text>().text = "RESPAWN";
			}
		}

		void Update() {
			if (bot && Input.GetMouseButtonDown(0)) {
				bot = false;
				return;
			}
			if (bot) {
				UpdateByBot();
			} else {
				UpdateByInput();
			}
		}

		void UpdateByInput() {
			player.inputHorizontal = Input.GetAxis("Horizontal");
			player.inputJump = Input.GetButtonDown("Jump");
			player.inputJumpHold = Input.GetButton("Jump");
			player.SetTrackingPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), true);
			player.flipped = Input.mousePosition.x < Camera.main.WorldToScreenPoint(player.transform.position).x;
			if (player.weaponInstance) {
				void PlayerShoot() {
					player.Shoot(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				}
				if (player.weaponInstance.IsAutomatic()) {
					if (Input.GetMouseButton(0)) {
						PlayerShoot();
						return;
					}
				} else if (player.weaponInstance.NeedCast()) {
					if (player.weaponInstance.IsCasting()) {
						if (Input.GetMouseButton(0)) {
							PlayerShoot();
							return;
						}
					} else {
						if (Input.GetMouseButtonDown(0)) {
							PlayerShoot();
							return;
						}
					}
				} else {
					if (Input.GetMouseButtonDown(0)) {
						PlayerShoot();
						return;
					}
				}
				player.weaponInstance.CancelShoot();
			}
		}

		void UpdateByBot() {
			if (botInputHorizontalTimer.Update()) {
				botHorizontal = Random.value < .5f ? -1 : 1;
			}
			player.inputHorizontal = botHorizontal;
			player.inputJump = false;
			if (botInputJumpTimer.Update()) {
				player.inputJump = true;
				botJump = Random.value;
			}
			botJump = Mathf.Max(0, botJump - Time.deltaTime);
			player.inputJumpHold = botJump > 0;
			if (botSelectEnemyTimer.Update()) {
				botCurrentEnemy = Spawner.GetRandomSpawnerObject<BatEnemy>();
			}
			if (botCurrentEnemy) {
				player.SetTrackingPosition(botCurrentEnemy.transform.position, false);
			} else {
				var offset = player.inputHorizontal > 0 ? Vector3.right : Vector3.left;
				player.SetTrackingPosition(transform.position + offset, false);
			}
			player.UpdateTrackingPosition();
			player.flipped = Camera.main.WorldToScreenPoint(player.GetTrackingPosition()).x < Camera.main.WorldToScreenPoint(player.transform.position).x;
			if (botInputShootTimer.Update()) {
				if (botCurrentEnemy) {
					player.Shoot(player.GetTrackingPosition());
				}
			}
		}
	}
}