using UnityEngine;

namespace MT.Packages.LD47
{
    public class Weapon : MonoBehaviour
    {
		[ReadOnly] public Character character;

		public int rank;
		public float maxAmmo = 10;
		[ReadOnly] public float currentAmmo;

		public ProjectilePool_Object projectile = null;
		[SerializeField] Audio.SoundEffect shootSoundEffect = null;
        [SerializeField] Transform front = null;

		[SerializeField] bool automatic = false;
		[SerializeField, Range(0, 3)] float shootInterval = .125f;
        [SerializeField] float restockPerSecond = 1.5f;
		[SerializeField, Range(0, 3)] float castTime = 0;

		[SerializeField] Weapon fallbackWeapon = null;
		
		[SerializeField, ReadOnly] float shootTimer;
		[SerializeField, ReadOnly] float castedTime;

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
			var direction = Utils.GetDirection2D(front.position, targetPosition);
			if (Pool.Get<ProjectilePool>(projectile).Spawn(ownerRingIndex, ownerFraction, front.position, direction)) {
				shootSoundEffect.Play(NetworkManager.self, front.position);
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