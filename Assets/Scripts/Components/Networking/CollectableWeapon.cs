using TNet;
using UnityEngine;

namespace MT.Packages.LD47
{
    public class CollectableWeapon : SpawnerObject
    {
        [SerializeField, ResourcePath("prefab")] string weapon = null;

		Weapon weaponInstance;

		protected override void Awake() {
			base.Awake();
			weaponInstance = Resources.Load<Weapon>(weapon);
		}

		protected override void OnHostAwake() {
			
		}

		protected override void OnHostUpdate() {
			
		}

		protected override void OnRemoteUpdate() {
			
		}

		void OnTriggerStay2D(Collider2D collision) {
			if (collision.TryGetComponent<Player>(out var player)) {
				if (player.tno.isMine && !player.IsDead()) {
					if (player.weaponInstance.rank > weaponInstance.rank) {
						return;
					}
					if (player.weapon == weapon) {
						if (player.weaponInstance.HasFullAmmo()) {
							return;
						}
						player.weaponInstance.FillAmmo();
					} else {
						player.weapon = weapon;
					}
					gameObject.DestroySelf();
				}
			}
		}
	}
}