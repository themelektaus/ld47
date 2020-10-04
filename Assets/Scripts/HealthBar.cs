using System.Collections;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
	RectTransform rectTransform;

	void Awake() {
		rectTransform = GetComponent<RectTransform>();
	}

	void Update() {
		rectTransform.anchorMax = new Vector2(Player2D.instance.GetHealthPercent(), 1);
	}
}