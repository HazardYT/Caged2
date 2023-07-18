using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    private FixedString32Bytes localPlayerName;

    private NetworkList<CharacterSelectState> players;


    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsClient){
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach(var character in allCharacters){
                var selectButtonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectButtonInstance.SetCharacter(this, character);
            }
            players.OnListChanged += HandlePlayersStateChanged;
            localPlayerName = SteamManager.steamName;
        }
        if (IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient){
            players.OnListChanged -= HandlePlayersStateChanged;
        }
        if (IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
       }
    }
    private void HandleClientConnected(ulong clientId){
        players.Add(new CharacterSelectState(clientId, localPlayerName));
    }
    private void HandleClientDisconnected(ulong clientId){
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }
    public void Select(Character character){
        characterNameText.text = character.DisplayName;

        characterInfoPanel.SetActive(true);

        SelectServerRpc(character.Id);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default){
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                players[i] = new CharacterSelectState(
                    players[i].ClientId,
                    players[i].name,
                    characterId
                );
                ServerManager.Instance.SetCharacter(players[i].ClientId, characterId);
            }
        }
    }
    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent){
        for (int i = 0; i < playerCards.Length; i++)
        {
            if(players.Count > i){
                playerCards[i].UpdateDisplay(players[i]);
            }
            else{
                playerCards[i].DisableDisplay();
            }
        }
    }
}
