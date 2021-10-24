using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class GameState_LevelSelection : GameState
	{
		[Core.Attributes.SceneName, UnityEngine.SerializeField]
		string levelSelectionScene = "Level Selection";

		protected override void Start() {
			Context.StartCoroutine(Core.Utility.LoadSceneRoutine(levelSelectionScene, true));
		}

		protected override void Exit() {
			SceneManager.UnloadSceneAsync(levelSelectionScene);
		}
	}
}