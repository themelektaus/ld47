
namespace MT.Packages.LD47
{
	public class Pool_Message_DisableAll : Mirror.MessageBase
	{
		public uint ownerID;

		public static explicit operator Pool_Message_DisableAll(uint ownerID) {
			return new Pool_Message_DisableAll { ownerID = ownerID };
		}

		public static void OnClientReceive(Pool_Message_DisableAll message) {
			if (Utility.IsMine(message.ownerID)) {
				return;
			}
			foreach (var pool in Pool.GetAll()) {
				pool.DisableAll(message.ownerID);
			}
		}
	}
}