using UnityEngine;

namespace MT.Packages.LD47
{
	public class ChangeRing : MonoBehaviour
	{
		public byte ringIndex;

		[SerializeField, ResourcePath("prefab")] string particleEffectName = null;
		[SerializeField] Audio.SoundEffect soundEffect = null;

		void Awake() {
			this.Logging(false);
		}

		void OnTriggerEnter2D(Collider2D collision) {
			if (collision.isTrigger) {
				return;
			}
			var character = collision.GetComponentInParent<Character>();
			if (character && character.hasAuthority) {
				if (character.GetRingIndex() == ringIndex) {
					return;
				}
				character.AbortJump();
				character.Remote_SetRingIndex(ringIndex);
				soundEffect.Play(NetworkManager.self);
				SpawnResourceMessage.Spawn(character.netId, particleEffectName, transform.position);
				this.Log($"{name} activated by character {character.netId}");
			}
		}
	}
}