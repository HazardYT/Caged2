using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Unity.Collections;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class SteamManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField LobbyIDInput;
    [SerializeField] private TMP_Text LobbyID;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject InLobbyMenu;
    [SerializeField] private GameObject startButton;
    [SerializeField] private Button mapButton;

    void OnEnable(){
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }
    void OnDisable(){
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
    }
    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
    }
    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.instance.currentLobby = lobby;
        LobbyID.text = lobby.Id.ToString();
        CheckUI();
        if (NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
        if (!IsHost) { startButton.SetActive(false); mapButton.interactable = false; }
    }
    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK){
            lobby.SetPublic();
            lobby.SetJoinable(true);
            ServerManager.Instance.StartHost();
        }
    }

    public async void HostLobby(){
        await SteamMatchmaking.CreateLobbyAsync(5);
    }
    public async void JoinLobbyWithID(){
        if (!ulong.TryParse(LobbyIDInput.text , out ulong ID)) 
            { return; }
        
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
        foreach(Lobby lobby in lobbies)
        {
            if (lobby.Id == ID) 
            {
                await lobby.Join();
                return;
            }
        }
    }
    public void CopyID(){
        TextEditor textEditor = new()
        {
            text = LobbyID.text
        };
        textEditor.SelectAll();
        textEditor.Copy();
    }
    public void LeaveLobby(){
        LobbySaver.instance.currentLobby?.Leave();
        LobbySaver.instance.currentLobby = null;
        NetworkManager.Singleton.Shutdown();
        CheckUI();
    }
    private void CheckUI(){
        if (LobbySaver.instance.currentLobby == null){
            MainMenu.SetActive(true);
            InLobbyMenu.SetActive(false);
        }
        else{
            MainMenu.SetActive(false);
            InLobbyMenu.SetActive(true);
        }
    }
    public void StartGameServer(){
        if (NetworkManager.Singleton.IsHost){
            ServerManager.Instance.StartGame();
        }
    }
}
