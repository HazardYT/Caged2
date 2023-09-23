using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SecurityMonitor : NetworkBehaviour
{
    public int selectedCamera = 0;
    public List<SecurityCamera> cameras = new();
    public Canvas screen;
    public RawImage rawImage;

    public void ToggleCamera(){
        cameras[selectedCamera].cam.enabled = false;
        if (selectedCamera == 0){
            selectedCamera++;
        }
        else if (selectedCamera >= cameras.Count -1){
            selectedCamera = 0;
        }
        cameras[selectedCamera].cam.enabled = true;
        rawImage.texture = cameras[selectedCamera].renderTexture;
    }
    public void ToggleNightVision(){

    }
    public void ToggleMonitor(bool oldValue, bool newValue){
        screen.enabled = newValue;
    }
    public void Start(){
        FuseBox.instance.FuseBoxState.OnValueChanged += ToggleMonitor;
        cameras[0].cam.enabled = true;
        rawImage.texture = cameras[0].renderTexture;
    }   
    public override void OnNetworkDespawn(){
        FuseBox.instance.FuseBoxState.OnValueChanged -= ToggleMonitor;
    }
}
