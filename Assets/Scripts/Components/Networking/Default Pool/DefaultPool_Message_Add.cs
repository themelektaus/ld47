using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool_Message_Add : Pool_Message
	{
		public Vector3 position;

		public static void OnServerReceive(DefaultPool_Message_Add message) {
			message.SendToClients();
		}

		public static void OnClientReceive(DefaultPool_Message_Add message) {
			if (!message.info.isMine) {
				message.UsePool<DefaultPool>((pool, info) => {
					pool.Spawn(info.ownerID, info.objectID, message.position);
				});
			}
		}
	}
}