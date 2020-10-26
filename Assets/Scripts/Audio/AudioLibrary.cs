using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace MT.Packages.LD47.Audio
{
	[CreateAssetMenu(menuName = "Audio/Audio Library")]
	public class AudioLibrary : ScriptableObject
	{
		public bool logging = true;
		public UnityEngine.Audio.AudioMixer audioMixer;

		public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
		public float minDistance = 15;
		public float maxDistance = 40;

		[System.Serializable]
		public class FootstepSoundTag
		{
			public string tag;
			public SoundEffect soundEffect;
		}
		public FootstepSoundTag[] footstepSoundTags;

		public SoundEffect GetFootstepSoundEffect(GameObject gameObject) {
			SoundEffect defaultSoundEffect = null;
			foreach (var soundTag in footstepSoundTags) {
				if (string.IsNullOrEmpty(soundTag.tag)) {
					defaultSoundEffect = soundTag.soundEffect;
				} else if (gameObject.CompareTag(soundTag.tag)) {
					return soundTag.soundEffect;
				}
			}
			return defaultSoundEffect;
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public static class AudioLibraryPlaymodeStateChanged
	{
		static AudioLibraryPlaymodeStateChanged() {
			EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged; ;
		}
		static void EditorApplication_playModeStateChanged(PlayModeStateChange state) {
			if (state == PlayModeStateChange.EnteredPlayMode) {
				Utils.UpdateSoundEffects(true);
			}
		}
	}
	class AudioLibraryBuildProcessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		public void OnPreprocessBuild(BuildReport report) {
			Utils.UpdateSoundEffects(true);
		}
	}
#endif

}