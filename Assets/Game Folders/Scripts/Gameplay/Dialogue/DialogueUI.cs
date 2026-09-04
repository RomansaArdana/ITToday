using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Typewriter Settings")]
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float characterDelay = 0.03f;

    private Coroutine typewriterCoroutine;
    private bool isTyping;

    public bool IsVisible => dialoguePanel != null && dialoguePanel.activeSelf;
    public bool IsTyping => isTyping;

    private void Awake()
    {
        Hide();
    }

    private void Update()
    {
        if (!IsVisible) return;
        if (!WasContinuePressed()) return;

        HandleContinueInput();
    }

    private void HandleContinueInput()
    {
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        DialogueManager.Instance?.ContinueDialogue();
    }

    private bool WasContinuePressed()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) return true;
            if (Keyboard.current.enterKey.wasPressedThisFrame) return true;
        }

        return false;
    }

    public void ShowNode(DialogueNode node)
    {
        if (node == null) return;

        StopTypewriter();

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        DialogueCharacterSO character = node.Speaker;

        UpdateSpeaker(character);
        UpdatePortrait(character, node.ExpressionOverride);

        if (dialogueText == null) return;

        if (!useTypewriter)
        {
            dialogueText.text = node.DialogueText;
            dialogueText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
            return;
        }

        StartTypewriter(node.DialogueText);
    }

    private void UpdateSpeaker(DialogueCharacterSO character)
    {
        if (speakerText == null) return;

        speakerText.text = character != null ? character.DisplayName : string.Empty;
    }

    private void UpdatePortrait(DialogueCharacterSO character, Sprite expressionOverride = null)
    {
        if (portraitImage == null) return;

        // Prioritas: expressionOverride > DefaultPortrait karakter
        Sprite portrait = expressionOverride != null
            ? expressionOverride
            : (character != null ? character.DefaultPortrait : null);

        portraitImage.sprite  = portrait;
        portraitImage.enabled = portrait != null;
    }

    private void StartTypewriter(string text)
    {
        if (dialogueText == null) return;

        if (string.IsNullOrEmpty(text))
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
            return;
        }

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(text));
    }

    private IEnumerator TypewriterRoutine(string text)
    {
        isTyping = true;

        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        yield return null;

        int characterCount = dialogueText.textInfo.characterCount;

        for (int i = 0; i <= characterCount; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            if (i < characterCount) yield return new WaitForSeconds(characterDelay);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;

        isTyping = false;
        typewriterCoroutine = null;
    }

    private void CompleteTyping()
    {
        if (!isTyping) return;

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = null;

        if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;

        isTyping = false;
    }

    private void StopTypewriter()
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = null;
        isTyping = false;

        if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
    }

    public void Hide()
    {
        StopTypewriter();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}