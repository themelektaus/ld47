using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(RectTransform))]
	public class HealthBar : MonoBehaviour
	{
		RectTransform rectTransform;
		PlayerController controller;

		void Awake() {
			rectTransform = GetComponent<RectTransform>();
		}

		void Update() {
			if (!controller) {
				controller = FindObjectOfType<PlayerController>();
			}
			if (controller) {
				// var p = controller.player;
				// TODO: rectTransform.anchorMax = new Vector2(p.currentHealth / p.health, 1);
			} else {
				// rectTransform.anchorMax = new Vector2(0, 1);
			}
		}
	}
}