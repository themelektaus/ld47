using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Message_Add : Pool_Message
	{
		public Vector3 position;
		public Vector2 direction;

		public static explicit operator ProjectilePool_Message_Add((Pool_ObjectInfo info, Vector3 position, Vector2 direction) v) {
			return new ProjectilePool_Message_Add { info = v.info, position = v.position, direction = v.direction };
		}

		public static void OnServerReceive(ProjectilePool_Message_Add message) {
			Utils.SendToClients(message);
		}

		public static void OnClientReceive(ProjectilePool_Message_Add message) {
			if (!message.info.isMine) {
				message.info.UsePool<ProjectilePool>((pool, info) => {
					pool.Spawn(info.ownerID, info.ownerRingIndex, info.ownerFraction, info.objectID, message.position, message.direction);
				});
			}
		}
	}
}