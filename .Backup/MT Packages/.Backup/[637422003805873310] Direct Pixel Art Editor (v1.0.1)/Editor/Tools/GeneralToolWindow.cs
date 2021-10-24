using MT.Packages.Core;
using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class GeneralToolWindow : ToolWindow
	{
		public GeneralToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "General";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(10000, 10000);
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(180, 80);
		}

		public override void DrawContent() {
			EditorGUI.BeginDisabledGroup(!owner.sourceTexture);
			if (GUILayout.Button("Center View", GetButtonStyle())) {
				owner.ResetView();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!owner.workingTexture || !owner.sourceTexture || owner.historyToolWindow.index < 2);
			GUI.color = new Color(.8f, 1f, .8f);
			if (GUILayout.Button("Apply", GetLeftButtonStyle())) {
				Apply();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!owner.workingTexture || !owner.sourceTexture || owner.historyToolWindow.entries.Count < 2);
			GUI.color = new Color(1f, .7f, .7f);
			owner.reloadSourceTexture = GUILayout.Button("Revert", GetRightButtonStyle());
			GUI.color = Color.white;
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		void Apply() {
			if (owner.sourceTexture.width != owner.workingTexture.width || owner.sourceTexture.height != owner.workingTexture.height) {
				string path = AssetDatabase.GetAssetPath(owner.sourceTexture);
				owner.sourceTexture = owner.workingTexture.Clone();
				System.IO.File.WriteAllBytes(path, owner.sourceTexture.EncodeToPNG());
				owner.historyToolWindow.ResetHistory();
				AssetDatabase.Refresh();
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			} else {
				Graphics.CopyTexture(owner.workingTexture, owner.sourceTexture);
				owner.sourceTexture.Apply();
				string assetPath = AssetDatabase.GetAssetPath(owner.sourceTexture);
				System.IO.File.WriteAllBytes(assetPath, owner.sourceTexture.EncodeToPNG());
				owner.historyToolWindow.ResetHistory();
				AssetDatabase.Refresh();
			}
		}
	}
}
