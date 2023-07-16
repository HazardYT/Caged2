using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySaver : MonoBehaviour
{
    public Steamworks.Data.Lobby? currentLobby;
    public static LobbySaver instance;
    void Awake(){
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
