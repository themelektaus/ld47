using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	public class GloryEnd : MonoBehaviour
	{
		[SerializeField] CanvasGroup canvasGroup = null;

		void OnTriggerEnter2D(Collider2D collision) {
			if (collision.TryGetComponent<Projectile>(out _)) {
				return;
			}
			if (collision.GetComponentInParent<PlayerController>()) {
				StartCoroutine(GameDone());
			}
		}

		IEnumerator GameDone() {
			yield return new WaitForSeconds(4);
			while (canvasGroup.alpha < 1) {
				canvasGroup.alpha += Time.deltaTime;
			}
		}
	}
}