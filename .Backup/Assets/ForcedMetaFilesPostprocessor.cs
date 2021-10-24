#if UNITY_EDITOR
using UnityEditor;
class ForcedMetaFilesPostprocessor : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (var importedAsset in importedAssets) {
			if (importedAsset.EndsWith(".meta.forced")) {
				System.IO.File.Copy(importedAsset, importedAsset.Substring(0, importedAsset.Length - 7), true);
			}
		}
	}
}
#endif