using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class NetworkRigidbody2D : NetworkBehaviour
	{
		[SerializeField] bool clientAuthority = false;

		[SyncVar] Vector2 targetVelocity;
		[SyncVar] Vector2 targetPosition;

		Rigidbody2D body;
		float nextSyncTime;
		Vector2 v1;
		Vector2 v2;

		bool clientWithAuthority => clientAuthority && hasAuthority;

		void Awake() {
			body = GetComponent<Rigidbody2D>();
		}

		void Update() {
			if (isServer) {
				targetVelocity = body.velocity;
				targetPosition = body.position;
			} else if (clientWithAuthority) {
				float now = Time.time;
				if (now > nextSyncTime) {
					nextSyncTime = now + syncInterval;
					Command_SendState(body.velocity, body.position);
				}
			}
		}

		[Command]
		void Command_SendState(Vector3 velocity, Vector3 position) {
			body.velocity = velocity;
			body.position = position;
			targetVelocity = velocity;
			targetPosition = position;
		}

		void FixedUpdate() {
			if (!isServer && !clientWithAuthority) {
				body.velocity = Vector2.SmoothDamp(body.velocity, targetVelocity, ref v1, syncInterval / 2);
				body.position = Vector2.SmoothDamp(body.position, targetPosition, ref v2, syncInterval / 2);
			}
		}
	}
}