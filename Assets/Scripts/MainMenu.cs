using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] GameObject mainMenu;
	[SerializeField] GameObject settingsMenu;
	[SerializeField] Slider masterVolumeSlider;
	[SerializeField] Slider musicVolumeSlider;
	[SerializeField] Slider sfxVolumeSlider;
	
	private void Start()
	{
		AudioManager.Singleton.PlayMusic(Music.Menu);
		NetManager.isHost = false;
		float f;
		AudioManager.Singleton.mixer.GetFloat("MasterVolume", out f);
		masterVolumeSlider.value = f;
		AudioManager.Singleton.mixer.GetFloat("MusicVolume", out f);
		musicVolumeSlider.value = f;
		AudioManager.Singleton.mixer.GetFloat("SfxVolume", out f);
		sfxVolumeSlider.value = f;
	}
	public void Quit()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, gameObject);
		StopAllCoroutines();
		Application.Quit();
	}
	public void Host()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, gameObject);
		NetManager.isHost = true;
		StopAllCoroutines();
		SceneManager.LoadScene("MainScene");
	}
	public void Join()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, gameObject);
		NetManager.isHost = false;
		StopAllCoroutines();
		SceneManager.LoadScene("MainScene");
	}
	public void Settings()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, gameObject);
		mainMenu.SetActive(false);
		settingsMenu.SetActive(true);
	}
	public void Back()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, gameObject);
		mainMenu.SetActive(true);
		settingsMenu.SetActive(false);
	}
	public void OnMasterVolumeChanged(float value)
	{
		AudioManager.Singleton.mixer.SetFloat("MasterVolume", masterVolumeSlider.value);
	}
	public void OnMusicVolumeChanged(float value)
	{
		AudioManager.Singleton.mixer.SetFloat("MusicVolume", musicVolumeSlider.value);
	}
	public void OnSFXVolumeChanged(float value)
	{
		AudioManager.Singleton.mixer.SetFloat("SfxVolume", sfxVolumeSlider.value);
	}
}
