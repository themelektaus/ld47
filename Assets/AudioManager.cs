using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance;

	[Range(0, 1)] public float musicVolume = 1;

	public AudioClip musicClip;

	public AudioMixerGroup audioMixerGroup;
	public AudioClip successClip;
	public AudioClip hitClip;
	public AudioClip playerHitClip;

	void Awake() {
		instance = this;
	}

	void Start() {
		Play(musicClip, musicVolume, 1, true);
	}

	public void Play(AudioClip clip, float volume, float pitch, bool loop = false) {
		var gameObject = new GameObject();
		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = audioMixerGroup;
		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.playOnAwake = false;
		audioSource.loop = loop;
		audioSource.Play();
		if (!loop) {
			var autoDestroy = gameObject.AddComponent<AutoDestroy>();
			autoDestroy.after = 2;
		}
	}

	public void PlaySuccess() {
		Play(successClip, .3f, 2);
	}

	public void PlayHit() {
		Play(hitClip, 1, 1);
	}

	public void PlayPlayerHit() {
		Play(playerHitClip, 1, .5f);
	}
}