using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public string currentMapSelected;
    public static ServerManager Instance { get; private set; }

    public Dictionary<ulong,ClientData> ClientData { get; private set; }

    private bool gameHasStarted;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
        }
        else{
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartHost(){
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        ClientData = new Dictionary<ulong, ClientData>();

        NetworkManager.Singleton.StartHost();
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response){
        if (ClientData.Count >= 5 || gameHasStarted){
            response.Approved = false;
            return;
        }
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);
        Debug.Log($"Added client {request.ClientNetworkId}");
    }
    private void OnNetworkReady(){
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }
    private void OnClientDisconnect(ulong clientId){
        if(ClientData.ContainsKey(clientId)){
            if(ClientData.Remove(clientId)){
                Debug.Log($"Removed client {clientId}");
            }
        }
    }
    public void SetCharacter(ulong clientId, int characterId){
        if (ClientData.TryGetValue(clientId, out ClientData data)){
            data.characterId = characterId;
        }
    }
    public void StartGame(){
        gameHasStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(currentMapSelected, LoadSceneMode.Single);
    }
}
