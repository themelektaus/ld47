
namespace MT.Packages.LD47
{
	public class Pool_Message_Remove : Pool_Message
	{
		public static void OnServerReceive(Pool_Message_Remove message) {
			message.SendToClients();
		}

		public static void OnClientReceive(Pool_Message_Remove message) {
			if (!message.info.isMine) {
				message.UsePool((pool, info) => {
					pool.Destroy(info);
				});
			}
		}
	}
}