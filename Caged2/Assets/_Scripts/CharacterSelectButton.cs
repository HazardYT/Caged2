using UnityEngine;
using UnityEngine.UI;
public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconimage;

    private CharacterSelectDisplay characterSelect;
    private Character character;

    public void SetCharacter(CharacterSelectDisplay characterSelect, Character character)
    {
        iconimage.sprite = character.Icon;

        this.characterSelect = characterSelect;
        this.character = character;
    }
    public void SelectCharacter(){
        characterSelect.Select(character);
    }
}
