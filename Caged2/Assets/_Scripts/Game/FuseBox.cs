using Unity.Netcode;
using UnityEngine;

public class FuseBox : NetworkBehaviour
{
    public static FuseBox instance;
    public NetworkVariable<bool> FuseBoxState = new(true);

    void Awake(){
        if (IsServer){
            FuseBoxState.Value = true;
        }
        if (instance == null) instance = this;
    }
    [ServerRpc]
    public void SetFuseBoxStateServerRpc(bool i){
        FuseBoxState.Value = i;
    }
}
