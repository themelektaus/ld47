
namespace MT.Packages.LD47
{
	public class Pool_Message_Disable : Pool_Message
	{
		public static explicit operator Pool_Message_Disable(Pool_ObjectInfo info) {
			return new Pool_Message_Disable { info = info };
		}

		public static void OnServerReceive(Pool_Message_Disable message) {
			Utility.SendToClients(message);
		}

		public static void OnClientReceive(Pool_Message_Disable message) {
			if (message.info.isMine) {
				return;
			}
			message.info.UsePool((pool, info) => {
				pool.Disable(info);
			});
		}
	}
}