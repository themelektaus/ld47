using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MT.Packages.LD47
{
    public class Button : MonoBehaviour, IPointerDownHandler
    {
		static readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();

		public static Button Get(string tag) {
			if (!buttons.ContainsKey(tag)) {
				return null;
			}
			return buttons[tag];
		}

		[SerializeField] bool active = true;

		public UnityEngine.Events.UnityEvent onClick = new UnityEngine.Events.UnityEvent();

		void Awake() {
			if (!CompareTag("Untagged")) {
				if (buttons.ContainsKey(tag)) {
					Debug.LogError("Button Tag \"{tag}\" already exists");
				}
				buttons.Add(tag, this);
			}
			if (!active) {
				gameObject.SetActive(false);
			}
		}

		void OnDestroy() {
			if (!CompareTag("Untagged")) {
				if (!buttons.ContainsKey(tag)) {
					Debug.LogError("Button Tag \"{tag}\" does not exists");
					return;
				}
				buttons.Remove(tag);
			}
		}

		public void OnPointerDown(PointerEventData eventData) {
			onClick.Invoke();
		}
	}
}