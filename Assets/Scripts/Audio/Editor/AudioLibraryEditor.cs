using System.Linq;
using UnityEditor;

namespace MT.Packages.LD47.Audio.Editor
{
	[CustomEditor(typeof(AudioLibrary))]
	public class AudioLibraryEditorD : UnityEditor.Editor
	{
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			DrawCustom();
		}

		readonly bool[] foldouts = new bool[4];
		bool IsAnythingFoldedOut {
			get {
				foreach (var foldout in foldouts) {
					if (foldout) {
						return true;
					}
				}
				return false;
			}
		}

		SerializedObject[] soundEffects;

		void DrawCustom() {
			if (UnityEngine.GUILayout.Button("Update Sound Effects")) {
				Utils.UpdateSoundEffects(target as AudioLibrary);
			}
			if (soundEffects == null || soundEffects.Length == 0) {
				soundEffects = EditorUtils.FindAndLoadAssets<SoundEffect>("t:SoundEffect")
					.Where(x => x.audioLibrary == target)
					.Select(x => new SerializedObject(x))
					.ToArray();
			}

			EditorGUILayout.Space();
			foldouts[0] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[0], "Mixer Control");
			if (foldouts[0]) {
				for (int i = 0; i < soundEffects.Length; i++) {
					DrawProperty(soundEffects[i], "audioMixerGroup", property => {
						EditorGUILayout.PropertyField(property, UnityEngine.GUIContent.none);
					});
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			EditorGUILayout.Space();
			foldouts[1] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[1], "Volume Control");
			if (foldouts[1]) {
				for (int i = 0; i < soundEffects.Length; i++) {
					DrawProperty(soundEffects[i], "volume", property => {
						var volumeValue = property.vector2Value;
						EditorGUILayout.MinMaxSlider(ref volumeValue.x, ref volumeValue.y, 0, 1);
						property.vector2Value = volumeValue;
					});
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			EditorGUILayout.Space();
			foldouts[2] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[2], "Pitch Control");
			if (foldouts[2]) {
				for (int i = 0; i < soundEffects.Length; i++) {
					DrawProperty(soundEffects[i], "pitch", property => {
						var pitchValue = property.vector2Value;
						if (UnityEngine.Mathf.Approximately(pitchValue.x, pitchValue.y)) {
							pitchValue.x = EditorGUILayout.Slider(pitchValue.x, 0, 2);
							pitchValue.y = pitchValue.x;
						} else {
							EditorGUILayout.MinMaxSlider(ref pitchValue.x, ref pitchValue.y, 0.25f, 2.0f);
						}
						property.vector2Value = pitchValue;
					});
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			EditorGUILayout.Space();
			foldouts[3] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[3], "Hold Time Control");
			if (foldouts[3]) {
				for (int i = 0; i < soundEffects.Length; i++) {
					DrawProperty(soundEffects[i], "holdTime", property => {
						property.floatValue = EditorGUILayout.Slider(property.floatValue, 0, 1);
					});
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			if (IsAnythingFoldedOut) {
				for (int i = 0; i < soundEffects.Length; i++) {
					soundEffects[i].ApplyModifiedProperties();
				}
			}
		}

		void DrawProperty(SerializedObject soundEffect, string propertyPath, System.Action<SerializedProperty> drawCallback) {
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField(soundEffect.targetObject, typeof(SoundEffect), false);
			EditorGUI.EndDisabledGroup();
			drawCallback(soundEffect.FindProperty(propertyPath));
			EditorGUILayout.EndHorizontal();
		}
	}
}