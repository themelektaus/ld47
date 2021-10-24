using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class GameState_Titlescreen : GameState
	{
		[Core.SceneName, UnityEngine.SerializeField]
		string titlescreenScene = "Titlescreen";

		protected override void Start() {
			Context.StartCoroutine(Core.Utility.LoadSceneRoutine(titlescreenScene, true));
		}

		protected override void Exit() {
			SceneManager.UnloadSceneAsync(titlescreenScene);
		}
	}
}