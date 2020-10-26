using UnityEngine;

namespace MT.Packages.LD47
{
    public class Weapon : MonoBehaviour
    {
		[ReadOnly] public Player player;

		public int rank;
		public float ammo = 10;
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

		Timer botShootTimer = (.34f, .68f);
		Timer botShootHoldTimer = (.3f, 5f);
		bool botShooting;

		void Awake() {
			FillAmmo();
		}

		public void FillAmmo() {
			currentAmmo = ammo;
		}

		void Update() {
			if (player.isServer) {
				return;
			}
			currentAmmo = Mathf.Min(currentAmmo + Time.deltaTime * restockPerSecond, ammo);
			shootTimer = Mathf.Max(0, shootTimer - Time.deltaTime);
			if (fallbackWeapon && currentAmmo < 1) {
				player.SetWeapon(fallbackWeapon.name);
			}
		}

		public void UpdateBotShoot(Vector3 targetPosition) {
			if (automatic) {
				if (!botShooting && botShootTimer.Update()) {
					botShooting = true;
				} else if (botShooting && botShootHoldTimer.Update()) {
					botShooting = false;
				}
				if (botShooting) {
					Shoot(targetPosition);
				}
				return;
			}
			if (castTime > 0) {
				Shoot(targetPosition);
				return;
			}
			if (botShootTimer.Update()) {
				Shoot(targetPosition);
			}
		}

		public void Shoot(Vector3 targetPosition) {
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
			if (Pool.Get<ProjectilePool>(projectile).Spawn(front.position, direction)) {
				shootSoundEffect.Play(this, front.position);
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