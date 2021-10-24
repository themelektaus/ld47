using MT.Packages.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class HistoryToolWindow : ToolWindow
	{
		public List<Texture2D> entries = new List<Texture2D>();
		public int index;

		public HistoryToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "History";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(10000, (int) owner.brushToolWindow.window.height + 15);
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(180, 80);
		}

		public override void DrawContent() {
			EditorGUILayout.LabelField($"History Index: {index} of {entries.Count}", EditorStyles.miniLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || index < 2);
			if (GUILayout.Button("Undo", GetLeftButtonStyle())) {
				index--;
				owner.workingTexture = entries[index - 1].Clone();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture || index == entries.Count);
			if (GUILayout.Button("Redo", GetRightButtonStyle())) {
				index++;
				owner.workingTexture = entries[index - 1].Clone();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		public void ResetHistory() {
			foreach (var entry in entries) {
				Object.DestroyImmediate(entry);
			}
			entries.Clear();
			index = 0;
			WriteHistory();
		}

		public void WriteHistory() {
			while (entries.Count - index > 0) {
				Texture2D texture = entries[entries.Count - 1];
				Object.DestroyImmediate(texture);
				entries.Remove(texture);
			}
			while (entries.Count >= 20) {
				Object.DestroyImmediate(entries[0]);
				entries.RemoveAt(0);
				index--;
			}
			entries.Add(owner.workingTexture.Clone());
			index++;
			owner.framesToolWindow.copyIndex = -1;
		}
	}
}
