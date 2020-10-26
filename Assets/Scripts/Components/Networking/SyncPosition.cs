using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class SyncPosition : NetworkBehaviour
	{
		public bool clientAuthoriy = true;

		[ReadOnly(duringPlayMode = true)]
		public float interval = 0.075f;

		Timer timer;
		SmoothTransformPosition position;

		void Awake() {
			timer = interval;
			position = (transform, interval * 1.5f);
		}

		void Update() {
			if (!hasAuthority && (clientAuthoriy || !isServer)) {
				position.Update();
				return;
			}
			if (!timer.Update()) {
				return;
			}
			if (clientAuthoriy) {
				SetPositionToServer(position.current);
			} else if (isServer) {
				SetPositionToClients(position.current);
			}
		}

		[Command]
		void SetPositionToServer(Vector3 targetPosition) {
			position.value = targetPosition;
			SetPositionToClients(targetPosition);
		}

		[ClientRpc(excludeOwner = true)]
		void SetPositionToClients(Vector3 targetPosition) {
			position.target = targetPosition;
		}
	}
}