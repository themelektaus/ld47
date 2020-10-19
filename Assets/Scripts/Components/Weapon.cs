using UnityEngine;

namespace MT.Packages.LD47
{
    public class Weapon : MonoBehaviour
    {
		[ReadOnly] public Player owner;

		public int rank;
		
		[SerializeField] Projectile projectilePrefab = null;
		[SerializeField] Audio.SoundEffect shootSoundEffect = null;
        [SerializeField] Transform front = null;

		[SerializeField] bool automatic = false;
		[SerializeField] float ammo = 10;
		[SerializeField, Range(0, 3)] float shootInterval = .125f;
        [SerializeField] float restockPerSecond = 1.5f;
		[SerializeField, Range(0, 3)] float castTime = 0;

		[SerializeField, ResourcePath("prefab")] string fallbackWeapon = null;
		
		[SerializeField, ReadOnly] float currentAmmo;
		[SerializeField, ReadOnly] float shootTimer;
		[SerializeField, ReadOnly] float castedTime;

		void Awake() {
			FillAmmo();
		}

		public void FillAmmo() {
			currentAmmo = ammo;
		}

		public bool HasFullAmmo() {
			return currentAmmo == ammo;
		}

		void Update() {
			currentAmmo = Mathf.Min(currentAmmo + Time.deltaTime * restockPerSecond, ammo);
			shootTimer = Mathf.Max(0, shootTimer - Time.deltaTime);
			if (!string.IsNullOrEmpty(fallbackWeapon) && currentAmmo < 1) {
				owner.weapon = fallbackWeapon;
			}
		}

		public void Shoot(string tag, Vector3 targetPosition) {
			if (shootTimer == 0 && currentAmmo >= 1) {
				if (castedTime < castTime) {
					castedTime += Time.deltaTime;
				} else {
					ShootNow(tag, targetPosition);
				}
			}
		}

		void ShootNow(string tag, Vector3 targetPosition) {
			castedTime = 0;
			shootTimer = shootInterval;
			currentAmmo--;
			if (NetworkPool.TryGet<Projectile, ProjectilePool>(projectilePrefab, out var pool)) {
				if (pool.Instantiate(tag, front.position, targetPosition)) {
					shootSoundEffect.Play(this, front.position);
				}
			}
		}

		public void CancelShoot() {
			castedTime = 0;
		}

		public bool IsAutomatic() {
			return automatic;
		}

		public bool NeedCast() {
			return castTime > 0;
		}

		public bool IsCasting() {
			return castedTime > 0;
		}
    }
}