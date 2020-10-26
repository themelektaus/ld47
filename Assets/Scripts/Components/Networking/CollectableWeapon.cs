using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class CollectableWeapon : NetworkBehaviour
	{
		[SerializeField, SyncVar] bool active = true;
		[SerializeField, ResourcePath("prefab")] string weaponName = null;
		int rank;
		float ammo;

		[SerializeField] SpriteRenderer sprite = null;
		[SerializeField, Range(1, 60)] float respawnDelay = 20;

		float respawnTimer;

		public override void OnStartServer() {
			base.OnStartServer();
			var weapon = Resources.Load<Weapon>(weaponName);
			rank = weapon.rank;
			ammo = weapon.ammo;
		}

		void Update() {
			sprite.enabled = active;
			if (!isServer) {
				return;
			}
			if (active) {
				respawnTimer = 0;
				return;
			}
			respawnTimer = Mathf.Max(0, respawnTimer - Time.deltaTime);
			if (respawnTimer == 0) {
				active = true;
			}
		}

		[ServerCallback]
		void OnTriggerStay2D(Collider2D collision) {
			if (active && collision.TryGetComponent<Player>(out var player)) {
				if (player.TryTakeWeapon(weaponName, rank, ammo)) {
					respawnTimer = respawnDelay;
					active = false;
				}
			}
		}
	}
}