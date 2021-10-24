using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MT.Packages.LD47
{
	public abstract class StartAs<T> : Singleton<T> where T : UnityEngine.MonoBehaviour
	{
		[Core.SceneName]
		[Core.ReadOnly(onlyDuringPlayMode = true)]
		[UnityEngine.SerializeField]
		string gameSceneName = "Game";

		void Start() {
			var scenes = new List<Scene>();
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				if (scene.name.StartsWith("Start as ")) {
					SceneManager.SetActiveScene(scene);
					continue;
				}
				scenes.Add(scene);
			}
			foreach (var scene in scenes) {
				SceneManager.UnloadSceneAsync(scene);
			}
			var gameScene = SceneManager.GetSceneByName(gameSceneName);
			if (gameScene.isLoaded) {
				SceneManager.SetActiveScene(gameScene);
				return;
			}
			SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive).completed += x => {
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(gameSceneName));
			};
		}
	}
}