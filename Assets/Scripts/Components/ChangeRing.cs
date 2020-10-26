using UnityEngine;

namespace MT.Packages.LD47
{
	public class ChangeRing : MonoBehaviour
	{
		[SerializeField] GameObject particleEffect = null;
		[SerializeField] Audio.SoundEffect soundEffect = null;
		[SerializeField, ReadOnly] bool activated = false;

		void OnTriggerEnter2D(Collider2D collision) {
			if (activated) {
				return;
			}
			if (collision.TryGetComponent<ProjectilePool_Object>(out _)) {
				return;
			}
			if (collision.GetComponentInParent<PlayerController>()) {
				soundEffect.Play(this);
				particleEffect.SetActive(true);
				activated = true;
				Debug.Log($"{this} activated");
			}
		}
	}
}