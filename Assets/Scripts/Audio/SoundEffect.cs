using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MT.Packages.LD47.Audio
{
    [CreateAssetMenu(menuName = "Audio/Sound Effect")]
    public class SoundEffect : ScriptableObject
    {
		[System.NonSerialized] static readonly Dictionary<AudioClip, float> holdedClips = new Dictionary<AudioClip, float>();

		[ReadOnly] public AudioLibrary audioLibrary;

		public AudioMixerGroup audioMixerGroup;

		[SerializeField, MinMax(0, 1)] Vector2 volume = new Vector2(.87f, .93f);
		[SerializeField, MinMax(0.25f, 2.0f)] Vector2 pitch = new Vector2(0.95f, 1.05f);

#pragma warning disable 649
		[SerializeField] AudioClip[] clips;
		[SerializeField] float holdTime;
#pragma warning restore 649

		AudioClip clip;

		public AudioClip GetRandomClip() {
			if (clips.Length == 0) {
				return null;
			}
			if (clips.Length == 1) {
				clip = clips[0];
			} else {
				AudioClip newClip;
				do {
					newClip = clips[Random.Range(0, clips.Length)];
				} while (clip == newClip);
				clip = newClip;
			}
			if (holdTime == 0) {
				return clip;
			}
			if (holdedClips.ContainsKey(clip)) {
				if (holdedClips[clip] > Time.unscaledTime) {
					return null;
				}
			}
			holdedClips[clip] = Time.unscaledTime + holdTime;
			return clip;
		}

		public float GetMaxVolume() {
			return volume.y;
		}

		public float GetRandomVolume() {
			return Random.Range(volume.x, volume.y);
		}

		public float GetRandomPitch() {
			return Random.Range(pitch.x, pitch.y);
		}

		public AudioSource Play(MonoBehaviour owner) {
			return Play(owner, owner.transform.position);
		}

		public AudioSource Play(MonoBehaviour owner, Vector3 position) {
			return Play(owner, position, 1);
		}

		public AudioSource Play(MonoBehaviour owner, Vector3 position, float volume) {
			return Play(owner, position, volume, 1);
		}

		public AudioSource Play(MonoBehaviour owner, Vector3 position, float volume, float pitch) {
			var soundObject = new GameObject(name);
			soundObject.transform.position = position;
			var audioSource = AddTo(soundObject, volume, pitch);
			if (audioSource) {
				IEnumerator Coroutine() {
					audioSource.clip = GetRandomClip();
					audioSource.Play();
					while (audioSource.isPlaying) {
						yield return null;
					}
					Destroy(audioSource.gameObject);
				}
				owner.StartCoroutine(Coroutine());
				return audioSource;
			}
			return null;
		}

		public AudioSource AddTo(MonoBehaviour owner) {
			return AddTo(owner.gameObject);
		}

		public AudioSource AddTo(GameObject owner) {
			return AddTo(owner, 1, 1);
		}

		public AudioSource AddTo(GameObject owner, float volume, float pitch) {
			var audioSource = owner.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.loop = false;
			UpdateAudioSource(audioSource, volume, pitch);
			return audioSource;
		}

		public void PlayThrough(AudioSource audioSource) {
			PlayThrough(audioSource, 1);
		}

		public void PlayThrough(AudioSource audioSource, float volume) {
			PlayThrough(audioSource, volume, 1);
		}

		public void PlayThrough(AudioSource audioSource, float volume, float pitch) {
			UpdateAudioSource(audioSource, volume, pitch);
			audioSource.clip = GetRandomClip();
			audioSource.Play();
		}

		void UpdateAudioSource(AudioSource audioSource, float volume, float pitch) {
			audioSource.dopplerLevel = 0;
			audioSource.spatialBlend = 1;
			audioSource.outputAudioMixerGroup = audioMixerGroup;
			audioSource.volume = GetRandomVolume() * volume;
			audioSource.pitch = GetRandomPitch() * pitch;
			audioSource.rolloffMode = audioLibrary.rolloffMode;
			audioSource.minDistance = audioLibrary.minDistance;
			audioSource.maxDistance = audioLibrary.maxDistance;
		}
	}
}