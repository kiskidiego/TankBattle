using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
	[SerializeField] GameObject settingsMenu;
	[SerializeField] Slider masterVolumeSlider;
	[SerializeField] Slider musicVolumeSlider;
	[SerializeField] Slider sfxVolumeSlider;
	NetPlayerController player;
	
	void Start()
	{
		float f;
		AudioManager.Singleton.mixer.GetFloat("MasterVolume", out f);
		musicVolumeSlider.value = f;
		AudioManager.Singleton.mixer.GetFloat("MusicVolume", out f);
		musicVolumeSlider.value = f;
		AudioManager.Singleton.mixer.GetFloat("SfxVolume", out f);
		sfxVolumeSlider.value = f;
	}
    public void SetPlayer(NetPlayerController player)
    {
		this.player = player;
	}
    public void Continue()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, player.gameObject);
		pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        gameObject.SetActive(false);
    }
    public void Settings()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, player.gameObject);
		pauseMenu.SetActive(false);
		settingsMenu.SetActive(true);
	}
    public void Quit()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, player.gameObject);
		player.DisconnectClient();
	}
	public void Back()
	{
		AudioManager.Singleton.PlaySfx(Sfx.Select, player.gameObject);
		pauseMenu.SetActive(true);
		settingsMenu.SetActive(false);
	}
    public void OnMasterVolumeChanged(float value)
    {
		AudioManager.Singleton.mixer.SetFloat("MasterVolume", musicVolumeSlider.value);
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
