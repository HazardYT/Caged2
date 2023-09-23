
using System.Collections.Generic;
using UnityEngine;

public class SecurityMonitorControls : MonoBehaviour, IInteractable
{
    public SecurityMonitor monitor;
    public List<AudioClip> _sounds = new();

    public void Interact(RaycastHit hit, Inventory inventory){
        if (FuseBox.instance.FuseBoxState.Value){            
            if (hit.transform.CompareTag("Keyboard")){
                monitor.ToggleCamera();
            }
            else if (hit.transform.CompareTag("Mouse")){
                monitor.ToggleNightVision();
            }
        }
    }
}