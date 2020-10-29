using UnityEngine;

namespace MT.Packages.LD47
{
	public class Exhauster : MonoBehaviour
	{
		[SerializeField] float velocityY = 1.2f;

		void OnTriggerStay2D(Collider2D collision) {
			if (collision.isTrigger) {
				return;
			}
			var character = collision.GetComponentInParent<Character>();
			if (character && character.hasAuthority) {
				character.attractor.velocityY = velocityY;
			}
		}
	}
}