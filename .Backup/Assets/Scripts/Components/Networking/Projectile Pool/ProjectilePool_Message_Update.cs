using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Message_Update : Pool_Message
	{
		public Vector3 position;

		public static explicit operator ProjectilePool_Message_Update((Pool_ObjectInfo info, Vector3 position) v) {
			return new ProjectilePool_Message_Update { info = v.info, position = v.position };
		}

		public static void OnServerReceive(ProjectilePool_Message_Update message) {
			Utility.SendToClients(message);
		}

		public static void OnClientReceive(ProjectilePool_Message_Update message) {
			if (!message.info.isMine) {
				message.info.UsePool<ProjectilePool>((pool, info) => {
					pool.UpdatePosition(info, message.position);
				});
			}
		}
	}
}