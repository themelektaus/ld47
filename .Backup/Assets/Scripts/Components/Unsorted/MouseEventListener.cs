using MT.Packages.Core;
using UnityEngine;
using UnityEngine.Events;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(Collider2D))]
	public class MouseEventListener : MonoBehaviour
	{
		[SerializeField] bool logging = false;
		[SerializeField] bool clickOnEnable = false;
		[SerializeField] UnityEvent onClick = new UnityEvent();
		[SerializeField] UnityEvent onEnter = new UnityEvent();
		[SerializeField] UnityEvent onExit = new UnityEvent();

		Collider2D _collider;

		void Awake() {
			this.Logging(logging);
		}

		void OnEnable() {
			if (clickOnEnable) {
				onClick.Invoke();
			}
		}

		void OnMouseDown() {
			this.Log("OnMouseDown()");
			onClick.Invoke();
		}

		void OnMouseEnter() {
			this.Log("OnMouseEnter()");
			onEnter.Invoke();
		}

		void OnMouseExit() {
			this.Log("OnMouseExit()");
			onExit.Invoke();
		}

		void OnDrawGizmos/*Selected*/() {
			if (!_collider) {
				_collider = GetComponent<Collider2D>();
			}
			var bounds = _collider.bounds;
			var size = bounds.size;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(bounds.center, size);

#if UNITY_EDITOR
			UnityEditor.Handles.BeginGUI();
			{
				var color = GUI.color;
				{
					var labelPosition = bounds.min;
					labelPosition.y += size.y;

					var labelStyle = new GUIStyle(GUI.skin.label) {
						fontSize = 24,
						alignment = TextAnchor.UpperLeft,
					};
					

					var text = Mathf.RoundToInt(transform.position.z * -10).ToString();
					var textPosition = new Vector2Int(9, 7);

					GUI.color = Color.black;
					for (int x = -1; x <= 1; x += 2) {
						for (int y = -1; y <= 1; y += 2) {
							labelStyle.contentOffset = new Vector2(x + textPosition.x, y + textPosition.y);
							UnityEditor.Handles.Label(labelPosition, text, labelStyle);
						}
					}

					GUI.color = Color.white;
					labelStyle.contentOffset = textPosition;
					UnityEditor.Handles.Label(labelPosition, text, labelStyle);
				}
				GUI.color = color;
			}
			UnityEditor.Handles.EndGUI();
#endif
		}
	}
}