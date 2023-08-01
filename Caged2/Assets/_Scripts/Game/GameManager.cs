using UnityEngine;
using TMPro;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> _gameTime = new NetworkVariable<float>(0);
    public Canvas _gameManagerCanvas;
    public TMP_Text _gameTimeText;
    public bool isStarted = false;
    public override void OnNetworkSpawn()
    {
        _gameTime.OnValueChanged += (float previousValue, float newValue) => {
            _gameTimeText.text = FormatTime(newValue);
        };
    }
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void ToggleTimer(){
        isStarted = !isStarted;
    }
    void FixedUpdate()
    {
        if (IsServer && isStarted)
        {
            _gameTime.Value += Time.fixedDeltaTime; 
        }
    }
}
