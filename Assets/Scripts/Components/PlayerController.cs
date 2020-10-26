using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Player))]
	public class PlayerController : MonoBehaviour
	{
		public static PlayerController instance;

		[ReadOnly] public Player player;

		public enum Mode
		{
			Spectator,
			InGameClientBot,
			InGamePlayer
		}

		[SerializeField] Mode mode = Mode.Spectator;
		[SerializeField, ReadOnly] Mode currentMode = Mode.Spectator;

		float botHorizontal;
		float botJump;
		readonly Timer botInputHorizontalTimer = (.6f, .9f);
		readonly Timer botInputJumpTimer = (2.5f, 4.2f);
		readonly Timer botSelectEnemyTimer = 1;
		BatEnemy botCurrentEnemy;

		SpectatorCamera spectatorCamera;
		Button respawnButton;
		Texture2D aimCursor32;
		Texture2D aimCursor64;

		Coroutine respawnCoroutine;

		void Awake() {
			if (instance) {
				Debug.LogError($"{instance} already exists");
				Destroy(this);
				return;
			}
			instance = this;
			player = GetComponent<Player>();
			player.attractor.body.mass = 1;
			player.onReceiveDamage.AddListener(damage => {
				player.ChangeHealth(-damage);
				CameraShake.Add(CameraControl.instance.playerReceiveDamageShake);
			});
			spectatorCamera = FindObjectOfType<SpectatorCamera>();
			respawnButton = Button.Get("Respawn Button");
			aimCursor32 = Resources.Load<Texture2D>("Aim Cursor 32");
			aimCursor64 = Resources.Load<Texture2D>("Aim Cursor 64");
			if (NetworkManager.botMode) {
				mode = Mode.InGameClientBot;
			} else {
				mode = Mode.InGamePlayer;
			}
		}

		void Update() {
			UpdateMode();
			switch (currentMode) {
				case Mode.Spectator:
					StopRespawnCoroutine();
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
					respawnButton.gameObject.SetActive(false);
					spectatorCamera.gameObject.SetActive(true);
					CameraControl.instance.SetTarget(spectatorCamera.transform, 5);
					break;

				case Mode.InGameClientBot:
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
					respawnButton.gameObject.SetActive(false);
					spectatorCamera.gameObject.SetActive(false);
					CameraControl.instance.SetTarget(player.transform, 30);
					if (player.isDead) {
						StartRespawnCoroutine();
					}
					UpdateByBot();
					break;

				case Mode.InGamePlayer:
					StopRespawnCoroutine();
					if (player.isDead) {
						Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
						respawnButton.gameObject.SetActive(true);
						spectatorCamera.gameObject.SetActive(true);
						CameraControl.instance.SetTarget(spectatorCamera.transform, 5);
						return;
					}
					if (Screen.width > 2160) {
						Cursor.SetCursor(aimCursor64, new Vector2(32, 32), CursorMode.ForceSoftware);
					} else {
						Cursor.SetCursor(aimCursor32, new Vector2(16, 16), CursorMode.ForceSoftware);
					}
					respawnButton.gameObject.SetActive(false);
					spectatorCamera.gameObject.SetActive(false);
					CameraControl.instance.SetTarget(player.transform, 30);
					UpdateByInput();
					break;
			}
		}

		void StartRespawnCoroutine() {
			if (respawnCoroutine == null) {
				IEnumerator RespawnRoutine() {
					yield return new WaitForSeconds(5);
					player.Respawn();
					respawnCoroutine = null;
				}
				respawnCoroutine = StartCoroutine(RespawnRoutine());
			}
		}

		void StopRespawnCoroutine() {
			if (respawnCoroutine != null) {
				StopCoroutine(respawnCoroutine);
				respawnCoroutine = null;
			}
		}

		void UpdateMode() {
			if (currentMode != mode) {
				currentMode = mode;
				switch (mode) {
					case Mode.Spectator:
						player.Command_SetInGame(false);
						player.Kill();
						break;
					default:
						player.Command_SetInGame(true);
						player.Respawn();
						break;
				}
			}
		}

		void UpdateByInput() {
			player.inputHorizontal = Input.GetAxis("Horizontal");
			player.inputJump = Input.GetButtonDown("Jump");
			player.inputJumpHold = Input.GetButton("Jump");
			player.trackingPosition.value = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
				botCurrentEnemy = FindObjectOfType<BatEnemy>();
			}
			if (botCurrentEnemy) {
				player.trackingPosition.value = botCurrentEnemy.transform.position;
			} else {
				var offset = player.inputHorizontal > 0 ? Vector3.right : Vector3.left;
				player.trackingPosition.value = transform.position + Vector3.up * .5f + offset * 4;
			}
			player.flipped = Camera.main.WorldToScreenPoint(player.trackingPosition.current).x < Camera.main.WorldToScreenPoint(player.transform.position).x;
			if (botCurrentEnemy && !botCurrentEnemy.dead) {
				player.UpdateBotShoot(player.trackingPosition.current);
			}
		}
	}
}