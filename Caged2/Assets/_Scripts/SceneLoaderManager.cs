using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class SceneLoaderManager : MonoBehaviour
{
    void Start()
    {
       StartCoroutine(LoadMenu()); 
    }

    IEnumerator LoadMenu(){
        yield return new WaitUntil (() => NetworkManager.Singleton != null);
        SceneManager.LoadScene("Menu");
    }
}
