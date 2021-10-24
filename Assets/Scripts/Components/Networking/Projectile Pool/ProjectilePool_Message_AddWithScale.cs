using UnityEngine;

namespace MT.Packages.LD47
{
	public class ProjectilePool_Message_AddWithScale : Pool_Message
	{
		public Vector3 position;
		public Vector2 direction;
		public Vector3 scale;

		public static explicit operator ProjectilePool_Message_AddWithScale((Pool_ObjectInfo info, Vector3 position, Vector2 direction, Vector3 scale) v) {
			return new ProjectilePool_Message_AddWithScale { info = v.info, position = v.position, direction = v.direction };
		}

		public static void OnServerReceive(ProjectilePool_Message_AddWithScale message) {
			Utility.SendToClients(message);
		}

		public static void OnClientReceive(ProjectilePool_Message_AddWithScale message) {
			if (!message.info.isMine) {
				message.info.UsePool<ProjectilePool>((pool, info) => {
					pool.Spawn(info.ownerID, info.ownerRingIndex, info.ownerFraction, info.objectID, message.position, message.direction, message.scale);
				});
			}
		}
	}
}