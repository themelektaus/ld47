using Mirror;

namespace MT.Packages.LD47
{
	public class NetworkObjectInfo : NetworkBehaviour, IObjectInfo
	{
		[SyncVar] public byte ringIndex;

		public byte GetRingIndex() {
			return ringIndex;
		}

		public void SetRingIndex(byte ringIndex) {
			this.ringIndex = ringIndex;
		}

		public override string ToString() {
			return $"{name} <NetworkObjectInfo>\r\n ringIndex: {ringIndex}";
		}
	}
}