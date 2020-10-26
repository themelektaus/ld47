using Mirror;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class SpawnResourceMessage : MessageBase
	{
		public uint ownerID;
		public string resourceName;
		public Vector3 position;

		public static void Spawn(uint ownerID, string resourceName, Vector3 position) {
			var message = new SpawnResourceMessage {
				ownerID = ownerID,
				resourceName = resourceName,
				position = position
			};
			if (NetworkClient.active) {
				NetworkClient.Send(message);
				return;
			}
			OnServerReceive(message);
		}

		public static void OnServerReceive(SpawnResourceMessage message) {
			var @object = Resources.Load<GameObject>(message.resourceName);
			var gameObject = Object.Instantiate(@object).ToTempInstance();
			gameObject.transform.position = message.position;
			if (gameObject.TryGetComponent<IOwner>(out var owner)) {
				owner.ownerID = message.ownerID;
			}
			NetworkServer.Spawn(gameObject);
		}
	}
}