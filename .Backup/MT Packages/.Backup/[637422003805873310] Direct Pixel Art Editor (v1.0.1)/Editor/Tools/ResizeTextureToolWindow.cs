using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class ResizeTextureToolWindow : ToolWindow
	{
		public TextAnchor anchor = TextAnchor.LowerLeft;
		public Vector2Int size;

		public ResizeTextureToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "Resize Texture";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(0, (int) (owner.position.height - owner.settingsToolWindow.window.height - window.height - 15));
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(160, 140);
		}

		public override void DrawContent() {
			size = EditorGUILayout.Vector2IntField("Size", size);
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField("Anchor", EditorStyles.miniLabel);
			anchor = (TextAnchor) EditorGUILayout.EnumPopup(anchor);
			EditorGUI.EndDisabledGroup();
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			int width = size.x;
			int height = size.y;
			EditorGUI.BeginDisabledGroup(width == 0 || height == 0 || !owner.workingTexture || (width == owner.workingTexture.width && height == owner.workingTexture.height));
			if (GUILayout.Button("Resize")) {
				var texture = DirectPixelArtEditorWindow.CreateTexture(width, height, owner.workingTexture, TextAnchor.LowerLeft);
				Object.DestroyImmediate(owner.workingTexture);
				owner.workingTexture = texture;
				owner.historyToolWindow.WriteHistory();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}
	}
}
