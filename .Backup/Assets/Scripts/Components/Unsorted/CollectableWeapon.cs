using Mirror;
using MT.Packages.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class CollectableWeapon : NetworkBehaviour
	{
		[SerializeField, SyncVar] bool active = true;
		[SerializeField, ResourcePath("prefab")] string weaponName = null;
		int weaponRank;

		[SerializeField] SpriteRenderer sprite = null;
		[SerializeField, Range(1, 60)] float respawnDelay = 20;
		[SerializeField, Range(0.05f, 1f)] float triggerInterval = .35f;

		float respawnTimer;
		readonly Timer triggerTimer = 1;
		readonly List<Character> characters = new List<Character>();

		public override void OnStartServer() {
			base.OnStartServer();
			weaponRank = Resources.Load<Weapon>(weaponName).rank;
			triggerTimer.SetInterval(triggerInterval);
		}

		void Update() {
			this.GetFromCache<Animator>().SetBool("Active", active);
			var color = sprite.color;
			color.a = active ? 1 : .3f;
			sprite.color = color;
			if (!isServer) {
				return;
			}
			if (active) {
				if (triggerTimer.Update()) {
					triggerTimer.SetInterval(triggerInterval);
					foreach (var character in characters) {
						if (character.clientAuthority) {
							character.TargetRpc_TryTakeWeapon(character.connectionToClient, netIdentity, weaponName, weaponRank);
						} else if (character.Server_TryTakeWeapon(weaponName, weaponRank)) {
							Server_TryCollect();
						}
					}
				}
			} else {
				respawnTimer = Mathf.Max(0, respawnTimer - Time.deltaTime);
				if (respawnTimer == 0) {
					active = true;
				}
			}
		}

		[ServerCallback]
		void OnTriggerEnter2D(Collider2D collision) {
			if (collision.TryGetComponent(out Character character)) {
				if (!characters.Contains(character)) {
					characters.Add(character);
				}
			}
		}

		[ServerCallback]
		void OnTriggerExit2D(Collider2D collision) {
			if (collision.TryGetComponent(out Character character)) {
				if (characters.Contains(character)) {
					characters.Remove(character);
				}
			}
		}

		[Command(ignoreAuthority = true)]
		public void Command_Collect(NetworkConnectionToClient conn = null) {
			if (Server_TryCollect()) {
				if (conn.identity.TryGetComponent(out Character character)) {
					character.TargetRpc_TakeWeapon(conn, weaponName);
				}
			}
		}

		[Server]
		public bool Server_TryCollect() {
			if (!active) {
				return false;
			}
			active = false;
			respawnTimer = respawnDelay;
			return true;
		}
	}
}