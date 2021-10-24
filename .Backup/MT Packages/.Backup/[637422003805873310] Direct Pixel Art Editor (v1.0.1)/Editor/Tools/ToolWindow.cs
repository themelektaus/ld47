using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.DPAE
{
	[System.Serializable]
	public abstract class ToolWindow
	{
		protected static GUIStyle GetWindowContentStyle() => new GUIStyle() {
			padding = new RectOffset(10, 12, 5, 10)
		};

		protected static GUIStyle GetButtonStyle() => new GUIStyle(GUI.skin.button) {
			margin = new RectOffset(20, 20, 3, 0),
			padding = new RectOffset(0, 0, 4, 4)
		};

		protected static GUIStyle GetFloatingButtonStyle() => new GUIStyle(GUI.skin.button) {
			margin = new RectOffset(2, 2, 0, 0),
			padding = new RectOffset(3, 3, 4, 4)
		};

		protected static GUIStyle GetLeftButtonStyle() => new GUIStyle(GUI.skin.button) {
			margin = new RectOffset(20, 2, 3, 0),
			padding = new RectOffset(0, 0, 4, 4)
		};

		protected static GUIStyle GetRightButtonStyle() => new GUIStyle(GUI.skin.button) {
			margin = new RectOffset(2, 20, 3, 0),
			padding = new RectOffset(0, 0, 4, 4)
		};

		public DirectPixelArtEditorWindow owner;

		public Rect window;
		public bool visible;

		public abstract string GetTitle();
		public abstract Vector2Int GetDefaultPosition();
		public abstract Vector2Int GetSize();
		public abstract void DrawContent();

		public ToolWindow(DirectPixelArtEditorWindow owner) {
			this.owner = owner;
			Vector2Int size = GetSize();
			window = new Rect(0, 0, size.x, size.y);
		}

		public void OnGUI() {
			int id = GUIUtility.GetControlID(FocusType.Passive);
			window.x = Mathf.Min(Mathf.Max(5, window.x), owner.position.width - window.width - 5);
			window.y = Mathf.Min(Mathf.Max(5, window.y), owner.position.height - window.height - 8);
			Vector2Int size = GetSize();
			window.width = size.x;
			window.height = size.y;
			var style = new GUIStyle(GUI.skin.window) {
				fontSize = 10,
				padding = new RectOffset(0, 0, 20, 0)
			};
			style.normal.background = Utility.CreateTexture(new Color(0, 0, 0, 0.2f));
			window = GUI.Window(id, window, OnGUI_Window, GetTitle(), style);
		}

		void OnGUI_Window(int windowID) {
			GUILayout.BeginVertical(GetWindowContentStyle());
			DrawContent();
			GUILayout.EndVertical();
			// GUI.DragWindow(new Rect(0, 0, 10000, 20));
		}

		// public void SetPositionNextTo(Rect otherWindow, int offsetY) {
		// 	window.x = otherWindow.x - window.width - 2;
		// 	if (window.x < 0) {
		// 		window.x += window.width + otherWindow.width + 2;
		// 	}
		// 	window.y = otherWindow.y + offsetY;
		// }
	}
}
