using UnityEngine;

namespace MT.Packages.LD47
{
	public class ObjectInfo : MonoBehaviour, IObjectInfo
	{
		public byte ringIndex;

		public byte GetRingIndex() {
			return ringIndex;
		}

		public void SetRingIndex(byte ringIndex) {
			this.ringIndex = ringIndex;
		}

		public override string ToString() {
			return $"{name} <ObjectInfo>\r\n ringIndex: {ringIndex}";
		}
	}
}