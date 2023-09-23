using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public Camera cam;
    public RenderTexture renderTexture;
    public int id;
    public void Start(){
        if (id == 1) { cam.enabled = true; }
    }
}
