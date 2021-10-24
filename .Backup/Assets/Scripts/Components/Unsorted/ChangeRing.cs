using MT.Packages.Core;
using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(ObjectInfo))]
	public class ChangeRing : MonoBehaviour
	{
		[SerializeField] bool logging = false;
		[SerializeField, ResourcePath("prefab")] string particleEffectName = null;
		[SerializeField] AudioSystem.SoundEffect soundEffect = null;

		void Awake() {
			this.Logging(logging);
		}

		void OnTriggerEnter2D(Collider2D collision) {
			if (collision.isTrigger) {
				return;
			}
			var character = collision.GetComponentInParent<Character>();
			if (character && character.hasAuthority) {
				if (character.GetRingIndex() == this.GetRingIndex()) {
					return;
				}
				character.AbortJump();
				character.Remote_SetRingIndex(this.GetRingIndex());
				character.Remote_Instantiate(particleEffectName, transform.position);
				soundEffect.Play(NetworkManager.instance, transform.position);
				this.Log($"Activated by character {character.netId}");
			}
		}
	}
}