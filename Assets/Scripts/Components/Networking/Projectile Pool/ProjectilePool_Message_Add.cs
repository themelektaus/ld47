using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Message_Add : Pool_Message
	{
		public Vector3 position;
		public Vector2 direction;

		public static void OnServerReceive(ProjectilePool_Message_Add message) {
			message.SendToClients();
		}

		public static void OnClientReceive(ProjectilePool_Message_Add message) {
			if (!message.info.isMine) {
				message.UsePool<ProjectilePool>((pool, info) => {
					pool.Spawn(info.ownerID, info.objectID, message.position, message.direction);
				});
			}
		}
	}
}