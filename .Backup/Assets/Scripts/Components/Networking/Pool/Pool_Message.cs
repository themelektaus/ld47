using Mirror;

namespace MT.Packages.LD47
{
	public abstract class Pool_Message : MessageBase
	{
		public Pool_ObjectInfo info;

		public override string ToString() {
			return $"{GetType().Name} <Info: {info}>";
		}
	}
}