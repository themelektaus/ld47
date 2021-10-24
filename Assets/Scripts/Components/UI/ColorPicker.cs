using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.LD47.UI
{
	[ExecuteInEditMode]
	public class ColorPicker : MonoBehaviour
	{
		public ColorPickerButton buttonPrefab;
		public Vector2Int size = new Vector2Int(5, 5);
		public Vector2 brightnessRange = new Vector2(.25f, 1.9f);

		public Core.Field target;

		void OnEnable() {
			if (Application.isPlaying) {
				return;
			}
			Core.Hash.Clear(this);
			Clear();
		}

		void OnDisable() {
			if (Application.isPlaying) {
				return;
			}
			Core.Hash.Clear(this);
			Clear();
		}

		void Clear() {
			var children = new List<GameObject>();
			foreach (Transform t in transform) {
				children.Add(t.gameObject);
			}
			foreach (var child in children) {
				if (Application.isPlaying) {
					Destroy(child);
				} else {
					DestroyImmediate(child);
				}
			}
		}

		void Update() {
			if (Application.isPlaying) {
				foreach (Transform child in transform) {
					if (child.TryGetComponent(out ColorPickerButton button)) {
						button.gameObject.SetActive(!string.IsNullOrEmpty(target.path));
					}
				}
				return;
			}
			if (!Core.Hash.HasChanged(this, new { buttonPrefab, size, brightnessRange }.GetHashCode())) {
				return;
			}
			Clear();
			if (!buttonPrefab || size.x <= 0 || size.y <= 0) {
				return;
			}
			for (var y = 0; y < size.y; y++) {
				for (var x = 0; x <= size.x; x++) {
					var button = Instantiate(buttonPrefab, transform);
					button.name = $"Button [{x}, {y}]";
					var rectTransform = button.GetComponent<RectTransform>();
					rectTransform.anchorMin = new Vector2(1f / (size.x + 1) * x, 1f / size.y * (size.y - 1 - y));
					rectTransform.anchorMax = new Vector2(1f / (size.x + 1) * (x + 1), 1f / size.y * (size.y - y));
					if (x == size.x) {
						var color = Color.white * Mathf.Lerp(brightnessRange.x, brightnessRange.y - 1, (float) y / size.y);
						color.a = 1;
						button.image.color = color;
					} else {
						button.image.color = GetColor((float) x / size.x, (float) y / size.y);
					}
#if UNITY_EDITOR
					var targetInfo = UnityEngine.Events.UnityEventBase.GetValidMethodInfo(
						this, nameof(SetColor), new[] { typeof(Color) }
					);
					var methodDelegate = System.Delegate.CreateDelegate(
						typeof(UnityEngine.Events.UnityAction<Color>), this, targetInfo
					) as UnityEngine.Events.UnityAction<Color>;
					UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, methodDelegate);
#endif
				}
			}
		}

		public void SetColor(Color color) {
			target.Set(color);
		}

		Color GetColor(float value, float brightness) {
			float r = Mathf.Abs(value * 6 - 3) - 1;
			float g = 2 - Mathf.Abs(value * 6 - 2);
			float b = 2 - Mathf.Abs(value * 6 - 4);
			Color color = new Color(
				Mathf.Clamp01(r),
				Mathf.Clamp01(g),
				Mathf.Clamp01(b)
			);
			var t1 = Mathf.Clamp01(brightness * (2 - brightnessRange.x) + brightnessRange.x);
			var t2 = Mathf.Clamp01(brightness - .5f) * brightnessRange.y;
			color.r = Mathf.Lerp(0, color.r, t1);
			color.g = Mathf.Lerp(0, color.g, t1);
			color.b = Mathf.Lerp(0, color.b, t1);
			color.r = Mathf.Lerp(color.r, 1, t2);
			color.g = Mathf.Lerp(color.g, 1, t2);
			color.b = Mathf.Lerp(color.b, 1, t2);
			return color;
		}

		public void SetTargetPath(string targetPath) {
			target.path = targetPath;
		}
	}
}