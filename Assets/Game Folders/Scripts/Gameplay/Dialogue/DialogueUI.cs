using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Button continueButton;

    public bool IsVisible => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        Hide();

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }

    public void ShowLine(DialogueLine line)
    {
        if (line == null)
        {
            return;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = line.SpeakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.DialogueText;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = line.Portrait;
            portraitImage.enabled = line.Portrait != null;
        }
    }

    public void Hide()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnContinueClicked()
    {
        DialogueManager.Instance?.ContinueDialogue();
    }
}