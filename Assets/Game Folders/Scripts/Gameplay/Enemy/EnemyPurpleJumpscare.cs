using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPurpleJumpscare : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Screen Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField, Range(0f, 1f)] private float flashAlpha = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpscareClip;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Coroutine flashRoutine;

    private GameObject flashObject;

    private void Awake()
    {
        enemyHealth ??=
            GetComponent<EnemyHealth>();

        if (flashImage != null)
        {
            flashObject =
                flashImage.gameObject;

            SetFlashAlpha(0f);

            flashObject.SetActive(false);
        }
    }

    public void PlayJumpscare()
    {
        if (enemyHealth != null &&
            !enemyHealth.IsAlive)
        {
            return;
        }

        PlayAudio();
        PlayFlash();

        if (enableDebugLog)
        {
            Debug.Log(
                "[Purple] JUMPSCARE FEEDBACK",
                this
            );
        }
    }

    private void PlayFlash()
    {
        if (flashImage == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine =
            StartCoroutine(
                FlashRoutine()
            );
    }

    private IEnumerator FlashRoutine()
    {
        if (flashObject != null)
        {
            flashObject.SetActive(true);
        }

        SetFlashAlpha(
            flashAlpha
        );

        yield return new WaitForSeconds(
            flashDuration
        );

        SetFlashAlpha(0f);

        if (flashObject != null)
        {
            flashObject.SetActive(false);
        }

        flashRoutine = null;
    }

    private void PlayAudio()
    {
        if (audioSource == null ||
            jumpscareClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            jumpscareClip
        );
    }

    private void SetFlashAlpha(
        float alpha)
    {
        if (flashImage == null)
        {
            return;
        }

        Color color =
            flashImage.color;

        color.a =
            Mathf.Clamp01(
                alpha
            );

        flashImage.color =
            color;
    }

    public void ForceHide()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(
                flashRoutine
            );

            flashRoutine = null;
        }

        SetFlashAlpha(0f);

        if (flashObject != null)
        {
            flashObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ForceHide();
    }
}