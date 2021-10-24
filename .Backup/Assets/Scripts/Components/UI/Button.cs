using UnityEngine;
using UnityEngine.EventSystems;

namespace MT.Packages.LD47.UI
{
    public class Button : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
		public UnityEngine.Events.UnityEvent onAwake = new UnityEngine.Events.UnityEvent();
		public UnityEngine.Events.UnityEvent onClick = new UnityEngine.Events.UnityEvent();

		Core.SmoothTransformLocalScale scale;
		UnityEngine.UI.Image uiImage;
		bool hover = false;

		void Awake() {
			onAwake.Invoke();
			scale = (transform, .05f);
			uiImage = GetComponent<UnityEngine.UI.Image>();
			Update();
		}

		void OnDisable() {
			hover = false;
		}

		void Update() {
			scale.target = Vector3.one * (hover ? 1.05f : 1);
			scale.Update();
			if (uiImage) {
				var color = uiImage.color;
				color.a = hover ? 1 : 0.2f;
				uiImage.color = color;
			}
		}

		public void OnPointerDown(PointerEventData eventData) {
			onClick.Invoke();
		}

		public static void Host() {
			NetworkManager.instance.isHost = true;
		}

		public static void Connect() {
			NetworkManager.instance.isHost = false;
		}

		public static void SetTrigger(string name) {
			NetworkManager.instance.SetTrigger(name);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData) {
			hover = false;
		}
	}
}