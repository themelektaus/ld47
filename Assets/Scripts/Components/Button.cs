using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MT.Packages.LD47
{
    public class Button : MonoBehaviour, IPointerDownHandler
    {
		public static List<Button> instances = new List<Button>();

		public static Button SetActiveByTag(string tag) {
			foreach (var instance in instances) {
				if (instance.CompareTag(tag)) {
					instance.gameObject.SetActive(true);
					return instance;
				}
			}
			return null;
		}

		public UnityEngine.Events.UnityEvent onClick = new UnityEngine.Events.UnityEvent();

		void Awake() {
			instances.Add(this);
		}

		void OnDestroy() {
			instances.Remove(this);
		}

		public void OnPointerDown(PointerEventData eventData) {
			onClick.Invoke();
		}

		public void SpawnPlayer() {
			// MultiplayerGame.instance.spawnPlayer = true;
		}
	}
}