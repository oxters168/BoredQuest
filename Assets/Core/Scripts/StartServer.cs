using UnityEngine;
using Mirror;

public class StartServer : MonoBehaviour
{
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        networkManager.StartServer();
    }
}
