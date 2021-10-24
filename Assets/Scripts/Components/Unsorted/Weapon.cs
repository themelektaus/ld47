using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
    public class Weapon : MonoBehaviour
    {
		[Core.Attributes.ReadOnly] public Character character;

		public int rank;
		public float maxAmmo = 10;
		[Core.Attributes.ReadOnly] public float currentAmmo;

		public ProjectilePool_Object projectile = null;
		[SerializeField] AudioSystem.SoundEffect shootSoundEffect = null;
        [SerializeField] Transform front = null;

		[SerializeField] bool automatic = false;
		[SerializeField, Range(0, 3)] float shootInterval = .125f;
        [SerializeField] float restockPerSecond = 1.5f;
		
		[SerializeField] Weapon fallbackWeapon = null;
		
		[SerializeField, Core.Attributes.ReadOnly] float shootTimer;

		[Range(0, 3)] public float castTime;
		[Core.Attributes.ReadOnly] public float castedTime;

		void Awake() {
			FillAmmo();
		}

		public void FillAmmo() {
			currentAmmo = maxAmmo;
		}

		void Update() {
			if (!character.hasAuthority) {
				return;
			}
			currentAmmo = Mathf.Min(currentAmmo + Time.deltaTime * restockPerSecond, maxAmmo);
			shootTimer = Mathf.Max(0, shootTimer - Time.deltaTime);
			if (fallbackWeapon && currentAmmo < 1) {
				character.Remote_SetWeapon(fallbackWeapon.name);
			}
		}

		public void Shoot(byte ownerRingIndex, byte ownerFraction, Vector3 targetPosition) {
			if (shootTimer > 0 || currentAmmo < 1) {
				return;
			}
			if (castedTime < castTime) {
				castedTime += Time.deltaTime;
				return;
			}
			castedTime = 0;
			shootTimer = shootInterval;
			currentAmmo--;
			var direction = Utility.GetDirection2D(front.position, targetPosition);
			var projectile = Pool.Get<ProjectilePool>(this.projectile).Spawn(ownerRingIndex, ownerFraction, front.position, direction);
			if (projectile) {
				var characterController = character.GetFromCache<CharacterController>(true);
				if (characterController) {
					projectile.Damage += (sender, e) => {
						CameraShake.Add(CameraControl.instance.enemyReceiveDamageShake);
					};
				}
				shootSoundEffect.Play(front.position);
			}
		}

		public void CancelShoot() {
			castedTime = 0;
		}

		public bool IsAutomatic() {
			return automatic;
		}

		public bool NeedsCast() {
			return castTime > 0;
		}

		public bool IsCasting() {
			return castedTime > 0;
		}
	}
}