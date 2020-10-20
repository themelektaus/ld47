
using Mirror;

namespace MT.Packages.LD47
{
	public class NetworkManager : Mirror.NetworkManager
	{
		public override void Awake() {
#if UNITY_EDITOR
			networkAddress = "localhost";
#endif
			base.Awake();
		}

		public override void OnServerReady(NetworkConnection conn) {
			base.OnServerReady(conn);
			NetworkServer.RegisterHandler<Projectile.SpawnMessage>(handler => {
				UnityEngine.Debug.Log(handler.objectID);
			});
		}
	}
}