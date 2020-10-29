using Mirror;
using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class AutoDestroy : MonoBehaviour
	{
		[SerializeField] float after;

		bool update;

		void Awake() {
			if (TryGetComponent(out NetworkIdentity identity) && identity.isServer) {
				Server_StartDestroyCoroutine();
				return;
			}
			update = true;
			Update();
		}

		[Server]
		void Server_StartDestroyCoroutine() {
			StartCoroutine(Server_DestroyRoutine());
		}

		[Server]
		IEnumerator Server_DestroyRoutine() {
			yield return new WaitForSeconds(after);
			NetworkServer.Destroy(gameObject);
		}

		void Update() {
			if (!update) {
				return;
			}
			if (after <= 0) {
				Destroy(gameObject);
			}
			after -= Time.deltaTime;
		}
	}
}