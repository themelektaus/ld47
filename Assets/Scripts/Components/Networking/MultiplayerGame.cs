using UnityEngine;
using S = UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class MultiplayerGame : Singleton<MultiplayerGame>
	{
		public static bool isReady {
			get {
				return false;
			}
		}

		public static bool isHosting {
			get {
				return false;
			}
		}

		public static bool isReadyAndHosting {
			get {
				if (isReady && isHosting) {
					return true;
				}
				return false;
			}
		}

		public static void SpawnPlayer() {
			
		}
	}
}