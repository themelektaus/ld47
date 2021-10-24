using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public class CreateNewTextureToolWindow : ToolWindow
	{
		public Vector2Int textureSize = new Vector2Int(16, 16);

		public CreateNewTextureToolWindow(DirectPixelArtEditorWindow owner) : base(owner) {

		}

		public override string GetTitle() {
			return "Create New Texture";
		}

		public override Vector2Int GetDefaultPosition() {
			return new Vector2Int(0, 0);
		}

		public override Vector2Int GetSize() {
			return new Vector2Int(160, 90);
		}

		public override void DrawContent() {
			textureSize = EditorGUILayout.Vector2IntField("Size", textureSize);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			int width = textureSize.x;
			int height = textureSize.y;
			EditorGUI.BeginDisabledGroup(width == 0 || height == 0);
			if (GUILayout.Button("Create")) {
				string path = EditorUtility.SaveFilePanel("Save PNG", "", "New Texture (" + width + "x" + height + ")", "png");
				if (!string.IsNullOrWhiteSpace(path)) {
					Texture2D texture = DirectPixelArtEditorWindow.CreateTexture(width, height, null, TextAnchor.LowerLeft);
					System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
					Object.DestroyImmediate(texture);
					AssetDatabase.Refresh();

					string assetPath = "Assets" + path.Substring(Application.dataPath.Length);

					TextureImporter textureAsset = (TextureImporter) AssetImporter.GetAtPath(assetPath);
					textureAsset.isReadable = true;
					textureAsset.textureCompression = TextureImporterCompression.Uncompressed;
					textureAsset.spritePixelsPerUnit = 16;
					textureAsset.filterMode = FilterMode.Point;
					textureAsset.SaveAndReimport();

					Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}
	}
}
