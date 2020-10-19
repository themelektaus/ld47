using UnityEngine;

namespace MT.Packages.LD47
{
	public class HealthBar : MonoBehaviour
	{
		RectTransform rectTransform;
		PlayerController playerController;

		void Awake() {
			rectTransform = GetComponent<RectTransform>();
		}

		void Update() {
			if (!playerController) {
				playerController = FindObjectOfType<PlayerController>();
			}
			float x;
			if (playerController) {
				var player = playerController.player;
				x = player.currentHealth / player.health;
			} else {
				x = 0;
			}
			rectTransform.anchorMax = new Vector2(x, 1);
		}
	}
}