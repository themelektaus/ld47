using System.Collections;
using UnityEngine;

public class GlorryEnd : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	void OnTriggerEnter2D(Collider2D collision) {
		StartCoroutine(GameDone());
	}

	IEnumerator GameDone() {
		yield return new WaitForSeconds(4);
		while (canvasGroup.alpha < 1) {
			canvasGroup.alpha += Time.deltaTime;
		}
	}
}