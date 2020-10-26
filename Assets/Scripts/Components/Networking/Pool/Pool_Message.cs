using Mirror;

namespace MT.Packages.LD47
{
	public abstract class Pool_Message : MessageBase
	{
		public Pool_ObjectInfo info;

		public void SendToAll() {
			SendToServer();
			SendToClients();
		}

		public void SendToServer() {
			if (NetworkClient.active) {
				NetworkClient.Send(this);
			}
		}

		public void SendToClients() {
			if (NetworkServer.active) {
				NetworkServer.SendToReady(this);
			}
		}

		protected void UsePool(System.Action<Pool, Pool_ObjectInfo> callback) {
			var pool = info.GetPool();
			if (pool) {
				callback.Invoke(pool, info);
			}
		}

		protected void UsePool<T>(System.Action<T, Pool_ObjectInfo> callback) where T : Pool {
			var pool = info.GetPool<T>();
			if (pool) {
				callback.Invoke(pool, info);
			}
		}

		public override string ToString() {
			return $"{GetType().Name} <Info: {info}>";
		}
	}
}