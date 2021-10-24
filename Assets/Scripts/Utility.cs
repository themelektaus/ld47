using UnityEngine;
using System.Linq;

namespace MT.Packages.LD47
{
	public static class Utility
	{
		public static Vector2 GetDirection2D(Vector2 a, Vector2 b) {
			return (b - a).normalized;
		}

		public static float GetAngle2D(Vector2 direction) {
			return GetAngle2D(Vector2.zero, direction);
		}

		public static float GetAngle2D(Vector2 a, Vector2 b) {
			return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg + 90;
		}

		public static float GetHorizontal(Transform a, Vector2 b, float distanceDamping, float minDistance, float deadzone) {
			var direction = GetDirection2D(a.position, b);
			direction = a.InverseTransformDirection(direction);
			float result = direction.x > 0 ? 1 : -1;
			if (distanceDamping == 0) {
				return result;
			}
			var magnitude = Mathf.Max(0.001f, ((Vector2) a.transform.position - b).magnitude);
			if (magnitude < minDistance) {
				return 0;
			}
			result *= Mathf.Clamp01(magnitude / distanceDamping);
			if (-deadzone < result && result < deadzone) {
				return 0;
			}
			return result;
		}

		public static bool TryGetMyID(out uint id) {
			id = 0;
			if (Mirror.NetworkServer.active) {
				return true;
			}
			if (Mirror.NetworkClient.active && Mirror.NetworkClient.connection.identity) {
				id = Mirror.NetworkClient.connection.identity.netId;
				return true;
			}
			return false;
		}

		public static bool IsMine(uint id) {
			return TryGetMyID(out var myID) && myID == id;
		}

		public static bool TryGetConnection(uint netId, out Mirror.NetworkConnectionToClient connection) {
			var connections = Mirror.NetworkServer.connections.Values;
			foreach (var _connection in connections.Where(x => x.identity && x.identity.netId == netId)) {
				connection = _connection;
				return true;
			}
			connection = null;
			return false;
		}

		public static bool Send(Mirror.IMessageBase message) {
			if (SendToServer(message, true)) {
				return true;
			}
			if (SendToClients(message, true)) {
				return true;
			}
			Debug.LogError($"Sending {message} failed :(");
			return false;
		}

		public static bool SendToServer(Mirror.IMessageBase message) {
			return SendToServer(message, false);
		}

		public static bool SendToServer(Mirror.IMessageBase message, bool ignoreErrors) {
			if (Mirror.NetworkClient.active) {
				Mirror.NetworkClient.Send(message);
				return true;
			}
			if (!ignoreErrors) {
				Debug.LogError($"Sending {message} to server failed :(");
			}
			return false;
		}

		public static bool SendToClients(Mirror.IMessageBase message) {
			return SendToClients(message, false);
		}

		public static bool SendToClients(Mirror.IMessageBase message, bool ignoreErrors) {
			if (Mirror.NetworkServer.active) {
				Mirror.NetworkServer.SendToReady(message);
				return true;
			}
			if (!ignoreErrors) {
				Debug.LogError($"Sending {message} to clients failed :(");
			}
			return false;
		}
	}
}