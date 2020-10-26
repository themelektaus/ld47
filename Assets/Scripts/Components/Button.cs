using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MT.Packages.LD47
{
    public class Button : MonoBehaviour, IPointerDownHandler
    {
		public static List<Button> instances = new List<Button>();

		public static Button Get(string tag) {
			foreach (var instance in instances) {
				if (instance.CompareTag(tag)) {
					return instance;
				}
			}
			return null;
		}

		[SerializeField] bool active = true;

		public UnityEngine.Events.UnityEvent onClick = new UnityEngine.Events.UnityEvent();

		void Awake() {
			instances.Add(this);
			if (!active) {
				gameObject.SetActive(false);
			}
		}

		void OnDestroy() {
			instances.Remove(this);
		}

		public void OnPointerDown(PointerEventData eventData) {
			onClick.Invoke();
		}
	}
}