
namespace MT.Packages.LD47
{
	public class Pool_Message_Remove : Pool_Message
	{
		public static explicit operator Pool_Message_Remove(Pool_ObjectInfo info) {
			return new Pool_Message_Remove { info = info };
		}

		public static void OnServerReceive(Pool_Message_Remove message) {
			Utility.SendToClients(message);
		}

		public static void OnClientReceive(Pool_Message_Remove message) {
			if (!message.info.isMine) {
				message.info.UsePool((pool, info) => {
					pool.Destroy(info);
				});
			}
		}
	}
}