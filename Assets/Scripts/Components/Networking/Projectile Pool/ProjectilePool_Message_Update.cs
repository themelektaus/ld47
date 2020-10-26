using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Message_Update : Pool_Message
	{
		public Vector3 position;

		public static void OnServerReceive(ProjectilePool_Message_Update message) {
			message.SendToClients();
		}

		public static void OnClientReceive(ProjectilePool_Message_Update message) {
			if (!message.info.isMine) {
				message.UsePool<ProjectilePool>((pool, info) => {
					pool.UpdatePosition(info, message.position);
				});
			}
		}
	}
}