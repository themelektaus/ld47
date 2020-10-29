using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class NetworkPosition : NetworkBehaviour
	{
		[ReadOnly(duringPlayMode = true)]
		[SerializeField] float interval = 0.075f;

		[SerializeField] bool clientAuthoriy = false;

		Timer timer;
		SmoothTransformPosition position;

		bool isMine { get { return hasAuthority || (!clientAuthoriy && isServer); } }

		void Awake() {
			timer = interval;
			position = (transform, interval * 1.5f);
		}

		void Update() {
			if (!isMine) {
				position.Update();
				return;
			}
			if (!timer.Update()) {
				return;
			}
			if (clientAuthoriy) {
				Command_SetTargetPositionToServer(position.current);
			} else if (isServer) {
				ClientRpc_SetTargetPositionToClients(position.current);
			}
		}

		[Command]
		void Command_SetTargetPositionToServer(Vector3 targetPosition) {
			position.value = targetPosition;
			ClientRpc_SetTargetPositionToClients(targetPosition);
		}

		[ClientRpc(excludeOwner = true)]
		void ClientRpc_SetTargetPositionToClients(Vector3 targetPosition) {
			position.target = targetPosition;
		}

		[Command]
		public void Command_SetPositionToServer(Vector3 position) {
			this.position.value = position;
			ClientRpc_SetPositionToClients(position);
		}

		[ClientRpc(excludeOwner = true)]
		public void ClientRpc_SetPositionToClients(Vector3 position) {
			SetPosition(position);
		}

		public void SetPosition(Vector3 position) {
			this.position.value = position;
		}
	}
}