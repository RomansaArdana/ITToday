using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System;

public class SettingPage : Page
{
    public Button b_home;
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown musicDropdown;
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer mainAudioMixer;

    private void Start()
    {
        b_home.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Menu));

        SetupMusicDropdown();

        musicDropdown.onValueChanged.AddListener(ChangeMusicTrack);

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            ChangeMusicVolume();
        }
    }

    private void SetupMusicDropdown()
    {
        musicDropdown.ClearOptions();

        List<string> musicOptions = new List<string> { "Music 1", "Music 2" };
        musicDropdown.AddOptions(musicOptions);

        musicDropdown.value = AudioManager.Instance.GetCurrentMusicChoice();
    }

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMusicTrack(int value)
    {
        AudioManager.Instance.ChangeMusicTrack(value);
    }

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        float volume = musicVol.value;
        mainAudioMixer.SetFloat("MusicVol", MathF.Log10(volume)*20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void ChangeSfxVolume()
    {
        float volume = sfxVol.value;
        mainAudioMixer.SetFloat("SfxVol", MathF.Log10(volume) * 20);
    }

    private void LoadVolume()
    {
        musicVol.value = PlayerPrefs.GetFloat("musicVolume");

        ChangeMusicVolume();
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
            Debug.Log("Open App");
    }
}