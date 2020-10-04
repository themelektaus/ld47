using UnityEngine;

public class CustomCursor : MonoBehaviour
{
	public Texture2D texture;
	public Vector2 hotspot;

	void Start() {
		if (texture) {
			Cursor.SetCursor(texture, hotspot, CursorMode.ForceSoftware);
		}
	}
}