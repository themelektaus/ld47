using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool_Message_Add : Pool_Message
	{
		public Vector3 position;

		public static explicit operator DefaultPool_Message_Add((Pool_ObjectInfo info, Vector3 position) v) {
			return new DefaultPool_Message_Add { info = v.info, position = v.position };
		}

		public static void OnServerReceive(DefaultPool_Message_Add message) {
			Utils.SendToClients(message);
		}

		public static void OnClientReceive(DefaultPool_Message_Add message) {
			if (message.info.isMine) {
				return;
			}
			message.info.UsePool<DefaultPool>((pool, info) => {
				pool.Spawn(info.ownerID, info.ownerRingIndex, info.objectID, message.position);
			});
		}
	}
}