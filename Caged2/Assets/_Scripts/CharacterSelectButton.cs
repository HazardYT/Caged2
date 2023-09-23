using UnityEngine;
using UnityEngine.UI;
public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconimage;
    [SerializeField] private Button button;
    [SerializeField] private GameObject disabledOverlay;
    private CharacterSelectDisplay characterSelect;
    public Character Character { get; private set; }
    public bool IsDisabled {get; private set;}

    public void SetCharacter(CharacterSelectDisplay characterSelect, Character character)
    {
        iconimage.sprite = character.Icon;

        this.characterSelect = characterSelect;
        
        Character = character;
    }
    public void SelectCharacter(){
        characterSelect.Select(Character);
    }
    public void SetDisabled(){
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }
}
