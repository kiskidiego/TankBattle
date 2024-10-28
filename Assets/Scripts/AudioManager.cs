using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] sfxClips;
    [SerializeField] private AudioClip[] musicClips;
    AudioSource musicSource;
    public static AudioManager Singleton;
	public AudioMixer mixer;
    public AudioMixerGroup sfxMixer;
	public AudioMixerGroup musicMixer;
    private void Awake()
    {
		if (Singleton == null)
        {
			Singleton = this;
            DontDestroyOnLoad(gameObject);
		}
		else
        {
			Destroy(gameObject);
		}
	}
    public void PlaySfx(Sfx sound, GameObject source)
    {
        AudioSource audioSource = new GameObject().AddComponent<AudioSource>();
		audioSource.clip = sfxClips[(int)sound];
        audioSource.gameObject.transform.position = source.transform.position;
        audioSource.outputAudioMixerGroup = sfxMixer;
        StartCoroutine(StopPlaying(audioSource));
		audioSource.Play();
	}
    public void PlayMusic(Music sound)
    {
		if (musicSource != null)
        {
			musicSource.Stop();
			Destroy(musicSource.gameObject);
		}
		musicSource = new GameObject().AddComponent<AudioSource>();
		musicSource.clip = musicClips[(int)sound];
		musicSource.loop = true;
		musicSource.outputAudioMixerGroup = musicMixer;
		musicSource.spatialize = false;
		musicSource.Play();
	}
    static IEnumerator StopPlaying(AudioSource audioSource)
    {
		yield return new WaitForSeconds(audioSource.clip.length);
		Destroy(audioSource.gameObject);
	}
}
public enum Sfx
{
	Select = 0,
	Pickup = 1,
	Explosion = 2,
}
public enum Music
{
	Menu = 0,
	Game = 1,
}