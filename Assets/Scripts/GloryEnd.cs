using System.Collections;
using UnityEngine;

public class GloryEnd : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	void OnTriggerEnter2D(Collider2D collision) {
		if (collision.TryGetComponent<Projectile>(out _)) {
			return;
		}
		if (collision.GetComponentInParent<Player2D>()) {
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