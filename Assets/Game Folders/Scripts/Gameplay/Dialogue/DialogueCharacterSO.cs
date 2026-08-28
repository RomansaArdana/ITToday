using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "The Day After/Dialogue/Character")]
public class DialogueCharacterSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private Sprite defaultPortrait;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public Sprite DefaultPortrait => defaultPortrait;
}