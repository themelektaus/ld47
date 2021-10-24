using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MT.Packages.LD47.UI
{
	public class ColorPickerButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public bool hover;
		public Image image;

		public UnityEngine.Events.UnityEvent<Color> onClick = new UnityEngine.Events.UnityEvent<Color>();

		void Update() {
			if (hover) {
				transform.SetSiblingIndex(transform.childCount - 1);
				transform.localScale = Vector3.one * 1.1f;
			} else {
				transform.localScale = Vector3.one;
			}
		}

		public void OnPointerDown(PointerEventData eventData) {
			onClick.Invoke(image.color);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData) {
			hover = false;
		}
	}
}