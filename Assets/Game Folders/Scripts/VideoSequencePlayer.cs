using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class VideoSequencePlayer : MonoBehaviour
{
    [Header("Video Sequence")]
    [Tooltip("Masukkan video secara berurutan (SCENE 1 sampai SCENE 5)")]
    [SerializeField] private VideoClip[] videoClips;

    [Header("Scene Transition")]
    [Tooltip("Nama scene yang akan di-load setelah semua video selesai (contoh: 02_1 Limbo_Train)")]
    [SerializeField] private string nextSceneName = "02_1 Limbo_Train";

    [Header("Controls")]
    [Tooltip("Pencet tombol ini untuk skip video yang sedang berjalan (contoh: Space)")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [Tooltip("Jika dicentang, skip akan langsung lompat ke Scene berikutnya. Jika tidak, skip ke video selanjutnya.")]
    [SerializeField] private bool skipDirectlyToNextScene = false;

    [Header("Environment Control")]
    [Tooltip("Pilih Canvas / UI yang ingin disembunyikan saat video berjalan")]
    [SerializeField] private GameObject uiCanvasToHide;
    
    [Tooltip("Pilih Player agar tidak bisa bergerak/bersuara selama video berjalan")]
    [SerializeField] private GameObject playerToDisable;

    private VideoPlayer videoPlayer;
    private int currentVideoIndex = 0;
    private bool isTransitioning = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Setup dasar VideoPlayer (bisa dioverride di Inspector)
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane; // Agar full screen
    }

    private void Start()
    {
        // Sembunyikan UI dan Player saat cutscene dimulai
        if (uiCanvasToHide != null)
        {
            uiCanvasToHide.SetActive(false);
        }
        
        if (playerToDisable != null)
        {
            playerToDisable.SetActive(false);
        }

        if (videoClips == null || videoClips.Length == 0)
        {
            Debug.LogWarning("[VideoSequencePlayer] Tidak ada video yang dimasukkan. Langsung load scene berikutnya.");
            LoadNextScene();
            return;
        }

        // Daftarkan event saat video selesai diputar
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // Mulai mainkan video pertama
        PlayVideo(0);
    }

    private void Update()
    {
        if (isTransitioning) return;

        if (Input.GetKeyDown(skipKey))
        {
            if (skipDirectlyToNextScene)
            {
                LoadNextScene();
            }
            else
            {
                PlayNextVideo();
            }
        }
    }

    private void PlayVideo(int index)
    {
        if (index >= videoClips.Length)
        {
            LoadNextScene();
            return;
        }

        currentVideoIndex = index;
        videoPlayer.clip = videoClips[currentVideoIndex];
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isTransitioning) return;
        PlayNextVideo();
    }

    private void PlayNextVideo()
    {
        currentVideoIndex++;
        PlayVideo(currentVideoIndex);
    }

    private void LoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.Stop();
        
        Debug.Log($"[VideoSequencePlayer] Semua video selesai. Memuat scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
