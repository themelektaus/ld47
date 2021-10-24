using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class GameState_Level : GameState
	{
		[Core.SceneName, UnityEngine.SerializeField]
		string mapScene = "";

		protected override void Start() {
			Context.StartCoroutine(Core.Utility.LoadSceneRoutine(mapScene, true, scene => {
				if (StartAsServer.exists) {
					NetworkManager.instance.StartServer();
				} else if (StartAsClient.exists) {
					if (NetworkManager.instance.isHost) {
						NetworkManager.instance.StartHost();
					} else {
						NetworkManager.instance.StartClient();
					}
				}
			}));
		}

		protected override void Update() {
			var controller = CharacterController.instance;
			Core.Cache.Find("Dead Menu").SetActive(controller && controller.character.isInGameAndDead);
		}

		protected override void Exit() {
			Core.Cache.Find("Dead Menu").SetActive(false);
			if (StartAsServer.exists) {
				// NetworkManager.instance.StopServer();
			} else if (StartAsClient.exists) {
				if (NetworkManager.instance.isHost) {
					NetworkManager.instance.StopHost();
				} else {
					NetworkManager.instance.StopClient();
				}
				
			}
			foreach (var pool in Pool.GetAll()) {
				pool.DestroyAll();
			}
			SceneManager.UnloadSceneAsync(mapScene);
		}
	}
}