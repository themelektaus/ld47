using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public class GameState_CharacterEditor : GameState
	{
		[Core.Attributes.SceneName, UnityEngine.SerializeField]
		string characterEditorScene = "Character Editor";

		protected override void Start() {
			CameraControl.instance.Reset(3);
			Context.StartCoroutine(Core.Utility.LoadSceneRoutine(characterEditorScene, true));
		}

		protected override void Exit() {
			SceneManager.UnloadSceneAsync(characterEditorScene);
			CameraControl.instance.Reset(10);
		}
	}
}