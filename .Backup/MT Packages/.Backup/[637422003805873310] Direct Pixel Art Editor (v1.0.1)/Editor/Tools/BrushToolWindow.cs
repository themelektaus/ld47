using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class BrushToolWindow : ToolWindow
	{
		public int type;
		public Color color = Color.black;
		public int size = 1;

		public BrushToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "Brush";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(10000, 0);
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(195, 240);
		}

		public override void DrawContent() {
			type = GUILayout.Toolbar(type, new[] { "Pencil", "Eraser", "Filler" }, GUILayout.Height(22));
			GUILayout.Space(2);
			EditorGUILayout.LabelField("Brush Size", EditorStyles.miniLabel);
			size = EditorGUILayout.IntSlider(size, 1, 10);
			GUILayout.Space(2);
			EditorGUILayout.LabelField("Brush Size", EditorStyles.miniLabel);
			color = EditorGUILayout.ColorField(color);
			GUILayout.Space(2);
			int x = 1;
			int y = 9;
			int steps = 3;
			var emptyStyle = new GUIStyle();
			for (int r = 0; r <= steps; r++) {
				for (int g = 0; g <= steps; g++) {
					for (int b = 0; b <= steps; b++) {
						var position = new Rect(x * 15, y * 15, 14, 14);
						var c = new Color(
							1f / steps * r,
							1f / steps * g,
							1f / steps * b
						);
						EditorGUI.DrawRect(position, c);
						if (GUI.Button(position, "", emptyStyle)) {
							color = c;
						}
						x++;
						if (x > 11) {
							x = 1;
							y++;
						}
					}
				}
			}
		}
	}
}
