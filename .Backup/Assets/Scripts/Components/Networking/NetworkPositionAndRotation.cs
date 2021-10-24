using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class NetworkPositionAndRotation : NetworkBehaviour
	{
		[Core.ReadOnly(onlyDuringPlayMode = true)]
		[SerializeField] float interval = 0.15f;
		[SerializeField] bool clientAuthoriy = false;

		Core.Timer timer;
		Core.SmoothTransformPosition position;
		Core.SmoothFloat rotation;

		void Awake() {
			timer = interval;
			position = (transform, interval * 1.5f);
			rotation = new Core.SmoothFloat(
				() => transform.localEulerAngles.z,
				x => {
					var eulerAngles = transform.localEulerAngles;
					eulerAngles.z = x;
					transform.localEulerAngles = eulerAngles;
				},
				interval * 1.5f
			);
		}

		void Update() {
			if (hasAuthority || (!clientAuthoriy && isServer)) {
				if (timer.Update()) {
					if (clientAuthoriy) {
						Command_SetTargetPositionToServer(position.current, rotation.current);
					} else if (isServer) {
						ClientRpc_SetTargetPositionToClients(position.current, rotation.current);
					}
				}
			} else {
				position.Update();
				rotation.Update(true);
			}
		}

		[Command]
		void Command_SetTargetPositionToServer(Vector3 targetPosition, float targetRotation) {
			position.value = targetPosition;
			rotation.value = targetRotation;
			ClientRpc_SetTargetPositionToClients(targetPosition, targetRotation);
		}

		[ClientRpc(excludeOwner = true)]
		void ClientRpc_SetTargetPositionToClients(Vector3 targetPosition, float targetRotation) {
			position.target = targetPosition;
			rotation.target = targetRotation;
		}
	}
}