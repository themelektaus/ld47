using UnityEngine;

namespace MT.Packages.LD47
{
	[System.Serializable]
	public class Cursor
	{
		public Texture2D texture;

		public Vector2 hotspot;

		public void Use() {
			if (!texture) {
				UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				return;
			}
			UnityEngine.Cursor.SetCursor(texture, hotspot, CursorMode.ForceSoftware);
		}
	}
}