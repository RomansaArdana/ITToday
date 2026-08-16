using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip music1;
    [SerializeField] private AudioClip music2;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu"; 

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClickSFX;

    private int currentMusicChoice = 0;
    private const string MUSIC_CHOICE_KEY = "MusicChoice";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSFXSource();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentMusicChoice = PlayerPrefs.GetInt(MUSIC_CHOICE_KEY, 0);
        PlaySelectedMusic();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainMenuSceneName)
        {
            StopMusic();
        }
        else if (!musicSource.isPlaying) 
        {
            PlaySelectedMusic();
        }

        SetupButtonSounds();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    private void SetupSFXSource()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    public void ChangeMusicTrack(int musicChoice)
    {
        currentMusicChoice = musicChoice;
        PlayerPrefs.SetInt(MUSIC_CHOICE_KEY, currentMusicChoice);
        PlayerPrefs.Save();
        PlaySelectedMusic();
    }

    private void PlaySelectedMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        musicSource.loop = true; 

        switch (currentMusicChoice)
        {
            case 0:
                musicSource.clip = music1;
                break;
            case 1:
                musicSource.clip = music2;
                break;
        }

        musicSource.Play();
    }


    public int GetCurrentMusicChoice()
    {
        return currentMusicChoice;
    }

    public void PlayButtonSFX()
    {
        if (buttonClickSFX != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSFX);
        }
    }

    public void SetupButtonSounds()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.RemoveListener(PlayButtonSFX);
            button.onClick.AddListener(PlayButtonSFX);
        }
    }
}