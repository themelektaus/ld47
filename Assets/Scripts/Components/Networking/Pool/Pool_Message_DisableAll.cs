
namespace MT.Packages.LD47
{
	public class Pool_Message_DisableAll : Mirror.MessageBase
	{
		public uint ownerID;

		public static void OnClientReceive(Pool_Message_DisableAll message) {
			if (!Utils.IsMine(message.ownerID)) {
				foreach (var pool in Pool.GetAll()) {
					pool.DisableAll(message.ownerID);
				}
			}
		}
	}
}