
namespace MT.Packages.LD47
{
	public class Pool_Message_Disable : Pool_Message
	{
		public static void OnServerReceive(Pool_Message_Disable message) {
			message.SendToClients();
		}

		public static void OnClientReceive(Pool_Message_Disable message) {
			if (!message.info.isMine) {
				message.UsePool((pool, info) => {
					pool.Disable(info);
				});
			}
		}
	}
}