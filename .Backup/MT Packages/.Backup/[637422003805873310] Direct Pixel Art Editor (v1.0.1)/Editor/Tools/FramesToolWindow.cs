using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class FramesToolWindow : ToolWindow
	{
		public Vector2Int grid = new Vector2Int(1, 1);
		public int index = 0;
		public int copyIndex = -1;
		public bool playing;
		public System.DateTime playingTime;
		public float playingFrameDuration = 0.2f;

		public FramesToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "Frames";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int((int) owner.settingsToolWindow.window.width + 10, 10000);
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(400, 90);
		}

		public override void DrawContent() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Grid", GUILayout.Width(40));
			int newGrid = EditorGUILayout.IntField(grid.x, GUILayout.Width(40));
			newGrid = Mathf.Max(1, newGrid);
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || grid == Vector2Int.one || playing);
			if (GUILayout.Button("Play", GetFloatingButtonStyle(), GUILayout.Width(45))) {
				Play();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || grid == Vector2Int.one || !playing);
			if (GUILayout.Button("Pause", GetFloatingButtonStyle(), GUILayout.Width(45))) {
				Pause();
			}
			EditorGUI.EndDisabledGroup();
			playingFrameDuration = EditorGUILayout.FloatField(playingFrameDuration, GUILayout.Width(30));
			playingFrameDuration = Mathf.Max(0.01f, playingFrameDuration);
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || grid == Vector2Int.one || copyIndex > -1);
			if (GUILayout.Button("Copy", GetFloatingButtonStyle(), GUILayout.Width(55))) {
				CopyFrame();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || grid == Vector2Int.one || copyIndex == -1 || copyIndex == index);
			if (GUILayout.Button("Paste", GetFloatingButtonStyle(), GUILayout.Width(55))) {
				PasteFrame();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			if (grid.x != newGrid) {
				grid.x = newGrid;
				copyIndex = -1;
				index = 0;
			}
			grid.y = newGrid;
			if (grid != Vector2Int.one) {
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("◄", GetFloatingButtonStyle())) {
					index--;
					if (index < 0) {
						index = 0;
					}
				}
				index = EditorGUILayout.IntSlider(index, 0, grid.x * grid.y - 1);
				if (GUILayout.Button("►", GetFloatingButtonStyle())) {
					index++;
					if (index > grid.x * grid.y - 1) {
						index = grid.x * grid.y - 1;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			UpdatePlay();
		}

		public Vector2Int GetIndexCoords() {
			return new Vector2Int(index % grid.x, index / grid.x);
		}

		public void CopyFrame() {
			copyIndex = index;
		}

		public void PasteFrame() {
			if (copyIndex > -1) {
				int frameWidth = owner.workingTexture.width / grid.x;
				int frameHeight = owner.workingTexture.height / grid.y;
				for (int x = 0; x < frameWidth; x++) {
					for (int y = 0; y < frameHeight; y++) {
						var color = owner.GetPixelColor(x + copyIndex % grid.x * frameWidth, y + copyIndex / grid.y * frameHeight);
						if (color.HasValue) {
							owner.SetPixelColor(x + index % grid.x * frameWidth, y + index / grid.y * frameHeight, color.Value);
						}
					}
				}
				owner.ApplyWorkingTextureChanges();
				owner.historyToolWindow.WriteHistory();
			}
		}

		public void Play() {
			playing = true;
			playingTime = System.DateTime.Now;
		}

		void UpdatePlay() {
			if (!playing) {
				return;
			}
			var currentPlayingTime = System.DateTime.Now;
			if ((currentPlayingTime - playingTime).TotalSeconds >= playingFrameDuration) {
				playingTime = playingTime.AddSeconds(playingFrameDuration);
				index++;
				if (index > grid.x * grid.y - 1) {
					index = 0;
				}
			}
		}

		public void Pause() {
			playing = false;
		}
	}
}
