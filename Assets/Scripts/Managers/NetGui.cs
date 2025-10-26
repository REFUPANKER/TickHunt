using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetGui : MonoBehaviour
{
    bool loaded;
    public void SelectOption(int opt)
    {
        //DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
        switch (opt)
        {
            case 1:
                //NetworkManager.Singleton.OnServerStarted += OnServerStarted;
                NetworkManager.Singleton.StartServer();
                break;
            case 2:
                //NetworkManager.Singleton.OnServerStarted += OnServerStarted;
                NetworkManager.Singleton.StartHost();
                break;
            case 3:
                NetworkManager.Singleton.StartClient();
                break;
        }
        gameObject.SetActive(false);
    }
    void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer && !loaded)
        {
            loaded = true;
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

    }
}