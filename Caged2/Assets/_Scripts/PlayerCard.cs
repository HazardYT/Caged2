using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Steamworks;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text characterNameText;

    public void UpdateDisplay(CharacterSelectState state){
        if (state.CharacterId != -1){
            Character character = characterDatabase.GetCharacterById(state.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
            characterNameText.text = character.DisplayName;
        }
        else{
            characterIconImage.enabled = false;
        }
        playerNameText.text = state.Name.ToString();
        visuals.SetActive(true);
    }
    public void DisableDisplay(){
        visuals.SetActive(false);
    }
}
