using UnityEngine;
using UnityEngine.Audio;

namespace MT.Packages.LD47
{
	public class Music : MonoBehaviour
	{
		[SerializeField] AudioMixerGroup group = null;
		[SerializeField] AudioClip clip = null;

		void Start() {
			var gameObject = new GameObject(clip.name);
			gameObject.transform.parent = transform.parent;
			var audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = group;
			audioSource.clip = clip;
			audioSource.playOnAwake = false;
			audioSource.loop = true;
			audioSource.Play();
		}
	}
}