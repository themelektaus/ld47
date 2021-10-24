using MT.Packages.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Character))]
	public class CharacterController : MonoBehaviour
	{
		public static CharacterController instance;

		[Core.Attributes.ReadOnly] public Character character;

		[SerializeField]
		[FormerlySerializedAs("aimCursor32")]
		Cursor aimCursorSmall = new Cursor {
			hotspot = new Vector2(16, 16)
		};

		[SerializeField]
		[FormerlySerializedAs("aimCursor48")]
		Cursor aimCursorMedium = new Cursor {
			hotspot = new Vector2(24, 24)
		};

		void Awake() {
			character = GetComponent<Character>();
		}

		void Start() {
			if (!character.hasAuthority) {
				Destroy(this);
				return;
			}
			if (character.isLocalPlayer) {
				this.SetSingleton(ref instance);
				StartLocalPlayer();
			}
			character.Remote_SetBot(false);
			Spawn();
		}

		void StartLocalPlayer() {
			character.onReceiveDamage.AddListener(damage => {
				character.Remote_AddHealth(-damage);
				CameraShake.Add(CameraControl.instance.characterReceiveDamageShake);
			});
			CameraControl.instance.SetTarget(transform, 20, .05f);
			if (Screen.width < 2160) {
				aimCursorSmall.Use();
			} else {
				aimCursorMedium.Use();
			}
		}

		public void Spawn() {
			var playerSpawnArea = GameObject.FindGameObjectWithTag("Player Spawn Area");
			if (playerSpawnArea) {
				character.Respawn(playerSpawnArea.transform.position, 0);
			} else {
				Debug.LogError("There is no GameObject tagged \"Player Spawn Area\" in the scene");
			}
		}

		void OnDestroy() {
			if (character.isLocalPlayer) {
				this.UnsetSingletone(ref instance);
				StopLocalPlayer();
			}
		}

		void StopLocalPlayer() {
			if (CameraControl.exists && character.hasAuthority && character.clientAuthority) {
				CameraControl.instance.SetTarget(null, 5, 5);
			}
			if (Screen.width < 2160) {
				aimCursorSmall.Unuse();
			} else {
				aimCursorMedium.Unuse();
			}
			
		}

		void Update() {
			if (!character.hasAuthority) {
				return;
			}
			if (!character.isDead) {
				UpdateController();
			}
		}

		void UpdateController() {
			character.inputHorizontal = Input.GetAxis("Horizontal");
			character.inputJump = Input.GetButtonDown("Jump");
			character.inputJumpHold = Input.GetButton("Jump");
			var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			character.aimPosition.value = mousePosition;
			if (character.weaponInstance) {
				if (character.weaponInstance.IsAutomatic()) {
					if (Input.GetMouseButton(0)) {
						character.Shoot(mousePosition);
						return;
					}
				} else if (character.weaponInstance.NeedsCast()) {
					if (character.weaponInstance.IsCasting()) {
						if (Input.GetMouseButton(0)) {
							character.Shoot(mousePosition);
							return;
						}
					} else {
						if (Input.GetMouseButtonDown(0)) {
							character.Shoot(mousePosition);
							return;
						}
					}
				} else {
					if (Input.GetMouseButtonDown(0)) {
						character.Shoot(mousePosition);
						return;
					}
				}
				character.weaponInstance.CancelShoot();
			}
		}
	}
}