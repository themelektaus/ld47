using UnityEngine;

public class ChangeRing : MonoBehaviour
{
	public int toRingIndex;
	public GameObject particleEffect;

	[HideInInspector] public bool activated;

	void OnTriggerEnter2D(Collider2D collision) {
		if (activated) {
			return;
		}
		if (collision.TryGetComponent<Projectile>(out _)) {
			return;
		}
		if (collision.GetComponentInParent<Player2D>()) {
			Player2D.instance.ringIndex = toRingIndex;
			AudioManager.instance.PlaySuccess();
			particleEffect.SetActive(true);
			activated = true;
		}
	}
}