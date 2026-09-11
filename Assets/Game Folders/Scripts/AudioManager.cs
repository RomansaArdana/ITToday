using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ── Audio Sources ──────────────────────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource otherSfxSource;

    // ── Music ──────────────────────────────────────────────────────────────────
    [Header("Music Clips")]
    [SerializeField] private AudioClip music1;
    [SerializeField] private AudioClip music2;

    // ── UI SFX ─────────────────────────────────────────────────────────────────
    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    // ── Gameplay SFX ───────────────────────────────────────────────────────────
    [Header("Player SFX")]
    [Tooltip("jump.mp3")]
    [SerializeField] private AudioClip jumpSFX;

    [Tooltip("Suara langkah kaki 1 (kiri)")]
    [SerializeField] private AudioClip footstep1SFX;

    [Tooltip("Suara langkah kaki 2 (kanan)")]
    [SerializeField] private AudioClip footstep2SFX;

    [Tooltip("dead 1.mp3")]
    [SerializeField] private AudioClip dead1SFX;

    [Tooltip("dead 2.mp3")]
    [SerializeField] private AudioClip dead2SFX;

    [Header("Combat SFX")]
    [Tooltip("ATTACK.mp3")]
    [SerializeField] private AudioClip attackSFX;

    [Tooltip("hit 1.mp3")]
    [SerializeField] private AudioClip hit1SFX;

    [Tooltip("hit 2.mp3")]
    [SerializeField] private AudioClip hit2SFX;

    [Tooltip("hit 3.mp3")]
    [SerializeField] private AudioClip hit3SFX;

    [Header("Cinematic / Story SFX")]
    [Tooltip("cry().mp3")]
    [SerializeField] private AudioClip crySFX;

    [Tooltip("scene 2 yg nangisnya].mp3")]
    [SerializeField] private AudioClip scene2CryingSFX;

    [Tooltip("all evil inara_.mp3")]
    [SerializeField] private AudioClip allEvilInaraSFX;

    [Tooltip("ending.mp3")]
    [SerializeField] private AudioClip endingSFX;

    // ── Other SFX (Looping) ────────────────────────────────────────────────────
    [Header("Other SFX (Looping)")]
    [Tooltip("SFX yang akan di-loop terus — cocok untuk ambient, background sound, dll")]
    [SerializeField] private AudioClip otherSfxClip;

    [Tooltip("Centang agar Other SFX langsung mulai saat game berjalan (seperti BGM)")]
    [SerializeField] private bool playOtherSfxOnStart = false;

    [Tooltip("Volume awal Other SFX")]
    [SerializeField] [Range(0f, 1f)] private float otherSfxVolume = 0.5f;

    [Tooltip("Jumlah perubahan volume tiap kali naik/turun")]
    [SerializeField] [Range(0.01f, 0.5f)] private float otherSfxVolumeStep = 0.1f;

    // ── Scene Settings ─────────────────────────────────────────────────────────
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    // ── Internal ───────────────────────────────────────────────────────────────
    private int currentMusicChoice = 0;
    private const string MUSIC_CHOICE_KEY = "MusicChoice";
    private Dictionary<string, AudioClip> sfxMap;

    // ══════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSources();
            BuildSFXMap();
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

    private void Start()
    {
        // Auto-play Other SFX jika dicentang
        if (playOtherSfxOnStart)
            PlayOtherSFX();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void BuildSFXMap()
    {
        sfxMap = new Dictionary<string, AudioClip>
        {
            // Player
            { "jump",            jumpSFX },
            { "dead_1",          dead1SFX },
            { "dead_2",          dead2SFX },
            { "death",           dead1SFX },

            // Combat
            { "attack",          attackSFX },
            { "hit",             hit1SFX },
            { "hit_1",           hit1SFX },
            { "hit_2",           hit2SFX },
            { "hit_3",           hit3SFX },

            // Cinematic
            { "cry",             crySFX },
            { "scene2_crying",   scene2CryingSFX },
            { "evil_inara",      allEvilInaraSFX },
            { "ending",          endingSFX },

            // UI
            { "button_click",    buttonClickSFX },
        };
    }

    // ── Public API: SFX ────────────────────────────────────────────────────────

    /// <summary>Putar SFX sekali. Contoh: AudioManager.Instance.PlaySFX("attack");</summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxSource == null) return;
        if (sfxMap == null) BuildSFXMap();

        if (!sfxMap.TryGetValue(sfxName, out AudioClip clip))
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' tidak ditemukan.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' belum di-assign di Inspector.");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXClip(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayRandomHit()
    {
        AudioClip[] hits = { hit1SFX, hit2SFX, hit3SFX };
        AudioClip[] valid = System.Array.FindAll(hits, c => c != null);
        if (valid.Length == 0) return;
        sfxSource.PlayOneShot(valid[Random.Range(0, valid.Length)]);
    }

    public void PlayRandomDeath()
    {
        AudioClip[] deaths = { dead1SFX, dead2SFX };
        AudioClip[] valid = System.Array.FindAll(deaths, c => c != null);
        if (valid.Length == 0) return;
        sfxSource.PlayOneShot(valid[Random.Range(0, valid.Length)]);
    }

    // ── Footstep (bergantian 1-2-1-2) ─────────────────────────────────────────
    private int footstepIndex = 0;

    /// <summary>
    /// Putar langkah kaki bergantian: kiri → kanan → kiri → kanan ...
    /// Panggil ini setiap langkah dari PlayerMovement.
    /// </summary>
    public void PlayFootstep()
    {
        if (sfxSource == null) return;

        AudioClip clip = footstepIndex % 2 == 0 ? footstep1SFX : footstep2SFX;
        footstepIndex++;

        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ── Public API: Other SFX (Looping) ───────────────────────────────────────

    /// <summary>Mulai putar Other SFX — akan loop terus sampai di-stop.</summary>
    public void PlayOtherSFX()
    {
        if (otherSfxSource == null || otherSfxClip == null) return;
        otherSfxSource.clip   = otherSfxClip;
        otherSfxSource.loop   = true;
        otherSfxSource.volume = otherSfxVolume;
        otherSfxSource.Play();
    }

    /// <summary>Putar Other SFX dengan clip tertentu (override clip).</summary>
    public void PlayOtherSFX(AudioClip clip)
    {
        if (otherSfxSource == null || clip == null) return;
        otherSfxSource.clip   = clip;
        otherSfxSource.loop   = true;
        otherSfxSource.volume = otherSfxVolume;
        otherSfxSource.Play();
    }

    /// <summary>Hentikan Other SFX.</summary>
    public void StopOtherSFX()
    {
        if (otherSfxSource != null) otherSfxSource.Stop();
    }

    /// <summary>Naikkan volume Other SFX sebesar satu step.</summary>
    public void IncreaseOtherSFXVolume()
    {
        if (otherSfxSource == null) return;
        otherSfxVolume = Mathf.Clamp01(otherSfxVolume + otherSfxVolumeStep);
        otherSfxSource.volume = otherSfxVolume;
        Debug.Log($"[AudioManager] Other SFX volume: {otherSfxVolume:F2}");
    }

    /// <summary>Kurangi volume Other SFX sebesar satu step.</summary>
    public void DecreaseOtherSFXVolume()
    {
        if (otherSfxSource == null) return;
        otherSfxVolume = Mathf.Clamp01(otherSfxVolume - otherSfxVolumeStep);
        otherSfxSource.volume = otherSfxVolume;
        Debug.Log($"[AudioManager] Other SFX volume: {otherSfxVolume:F2}");
    }

    /// <summary>Set volume Other SFX langsung ke nilai tertentu (0–1).</summary>
    public void SetOtherSFXVolume(float value)
    {
        if (otherSfxSource == null) return;
        otherSfxVolume = Mathf.Clamp01(value);
        otherSfxSource.volume = otherSfxVolume;
    }

    // ── Music ──────────────────────────────────────────────────────────────────

    public void ChangeMusicTrack(int musicChoice)
    {
        currentMusicChoice = musicChoice;
        PlayerPrefs.SetInt(MUSIC_CHOICE_KEY, currentMusicChoice);
        PlayerPrefs.Save();
        PlaySelectedMusic();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public int GetCurrentMusicChoice() => currentMusicChoice;

    private void PlaySelectedMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.loop = true;

        switch (currentMusicChoice)
        {
            case 0: musicSource.clip = music1; break;
            case 1: musicSource.clip = music2; break;
        }

        musicSource.Play();
    }

    // ── UI ─────────────────────────────────────────────────────────────────────

    public void PlayButtonSFX() => PlaySFX("button_click");

    public void SetupButtonSounds()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.RemoveListener(PlayButtonSFX);
            button.onClick.AddListener(PlayButtonSFX);
        }
    }

    // ── Scene ──────────────────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainMenuSceneName)
            StopMusic();
        else if (!musicSource.isPlaying)
            PlaySelectedMusic();

        SetupButtonSounds();
    }

    // ── Setup ──────────────────────────────────────────────────────────────────

    private void SetupSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        if (otherSfxSource == null)
        {
            otherSfxSource = gameObject.AddComponent<AudioSource>();
            otherSfxSource.playOnAwake = false;
            otherSfxSource.loop = true;
            otherSfxSource.volume = otherSfxVolume;
        }
    }
}