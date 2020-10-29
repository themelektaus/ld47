using UnityEngine;

namespace MT.Packages.LD47
{
	public class DefaultPool_Message_Update : Pool_Message
	{
		public Vector3 position;

		public static explicit operator DefaultPool_Message_Update((Pool_ObjectInfo info, Vector3 position) v) {
			return new DefaultPool_Message_Update { info = v.info, position = v.position };
		}

		public static void OnServerReceive(DefaultPool_Message_Update message) {
			Utils.SendToClients(message);
		}

		public static void OnClientReceive(DefaultPool_Message_Update message) {
			if (!message.info.isMine) {
				message.info.UsePool<DefaultPool>((pool, info) => {
					pool.UpdatePosition(message.info, message.position);
				});
			}
		}
	}
}