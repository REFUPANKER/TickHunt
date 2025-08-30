using UnityEngine;
using Unity.Netcode;

public class NetGui : MonoBehaviour
{   
    public void SelectOption(int opt)
    {
        switch (opt)
        {
            case 1:
                NetworkManager.Singleton.StartServer();
                break;
            case 2:
                NetworkManager.Singleton.StartHost();
                break;
            case 3:
                NetworkManager.Singleton.StartClient();
                break;
        }
        gameObject.SetActive(false);
    }
}