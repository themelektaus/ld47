using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class GameState_OptionsMenu : GameState
	{
		[Core.Attributes.SceneName, UnityEngine.SerializeField]
		string optionsMenuScene = "Options Menu";

		protected override void Start() {
			Context.StartCoroutine(Core.Utility.LoadSceneRoutine(optionsMenuScene, true));
		}

		protected override void Exit() {
			SceneManager.UnloadSceneAsync(optionsMenuScene);
		}
	}
}