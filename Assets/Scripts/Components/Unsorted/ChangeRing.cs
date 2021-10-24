using UnityEngine;

namespace MT.Packages.LD47
{
	[RequireComponent(typeof(ObjectInfo))]
	public class ChangeRing : MonoBehaviour
	{
		[SerializeField, Core.Attributes.ResourcePath("prefab")] string particleEffectName = null;
		[SerializeField] AudioSystem.SoundEffect soundEffect = null;

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
				soundEffect.Play(transform.position);
				this.Log($"Activated by character {character.netId}");
			}
		}
	}
}