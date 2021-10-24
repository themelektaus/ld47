using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class SettingsToolWindow : ToolWindow
	{
		public Color transparentBackgroundColor = new Color(.2f, .3f, .3f, .2f);

		public SettingsToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "Settings";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(0, (int) (owner.position.height - window.height));
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(180, 80);
		}

		public override void DrawContent() {
			EditorGUILayout.LabelField("Transparent Background Color", EditorStyles.miniLabel);
			transparentBackgroundColor = EditorGUILayout.ColorField(transparentBackgroundColor);
		}
	}
}
