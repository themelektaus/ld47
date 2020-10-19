using UnityEngine;

namespace MT.Packages.LD47.Audio
{
	public class SoundEffectComponent : MonoBehaviour
	{
		public SoundEffect soundEffect;

		AudioSource audioSource;

		void Awake() {
			audioSource = soundEffect.AddTo(this);
		}

		void OnEnable() {
			soundEffect.PlayThrough(audioSource);
		}
	}
}