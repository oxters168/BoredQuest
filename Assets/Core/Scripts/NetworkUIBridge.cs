using UnityEngine;
using UnityEngine.Events;
using Barebones.MasterServer;
using Barebones.Networking;
using Mirror;
using System.Linq;

[System.Serializable]
public class ConnectionEvent : UnityEvent<bool> {}

[System.Serializable]
public class RoomEvent : UnityEvent<string, bool> {}

public class NetworkUIBridge : MonoBehaviour
{
    private NetworkManager networkManager;
    private TelepathyTransport telepathyTransport;
    private Mirror.Websocket.WebsocketTransport websocketTransport;

    [Space(10)]
    public UnityEngine.UI.Selectable[] enabledOnlyIfConnected;

    [Space(10)]
    public ConnectionEvent OnMSFConnectionChanged;
    public ConnectionEvent OnClientConnectionChanged;
    public ConnectionEvent OnMSFSignedIn;
    public RoomEvent OnRoomFinalized;

    private bool prevMSFConnected;
    private bool prevClientConnected;
    private bool prevMSFSignedIn;

    void Awake()
    {
        SetInteractables(false);
        networkManager = FindObjectOfType<NetworkManager>();
        telepathyTransport = networkManager.GetComponent<TelepathyTransport>();
        websocketTransport = networkManager.GetComponent<Mirror.Websocket.WebsocketTransport>();
    }
    void Update()
    {
        CheckChanges();
    }

    private void SetInteractables(bool value)
    {
        foreach (var mb in enabledOnlyIfConnected)
            mb.interactable = value;
    }
    private void CheckChanges()
    {
        bool isMSFConnected = Msf.Connection.IsConnected;
        if (prevMSFConnected != isMSFConnected)
            MSFConnectionChanged(isMSFConnected);

        bool isClientConnected = NetworkClient.isConnected;
        if (prevClientConnected != isClientConnected)
            ClientConnectionChanged(isClientConnected);

        bool isSignedIn = Msf.Client.Auth.IsSignedIn;
        if (prevMSFSignedIn != isSignedIn)
            MSFSignedInChanged(isSignedIn);

        prevMSFConnected = isMSFConnected;
        prevClientConnected = isClientConnected;
        prevMSFSignedIn = isSignedIn;
    }
    private void MSFConnectionChanged(bool isConnected)
    {
        SetInteractables(isConnected);
        if (isConnected)
            Msf.Client.Auth.SignInAsGuest(SignInHandler);

        OnMSFConnectionChanged?.Invoke(isConnected);
    }
    private void ClientConnectionChanged(bool isConnected)
    {
        OnClientConnectionChanged?.Invoke(isConnected);
    }
    private void MSFSignedInChanged(bool isSignedIn)
    {
        OnMSFSignedIn?.Invoke(isSignedIn);
    }
    private void RoomFinalized(string roomName, bool successfully)
    {
        Debug.Log("Room finalized with status " + successfully);
        if (successfully)
        {
            Debug.Log("Requesting games");
            MsfTimer.WaitForSeconds(0.2f, () =>
            {
                Msf.Client.Matchmaker.FindGames((games) =>
                {
                    Debug.Log("Received " + games.Count() + " game(s)");
                    var recentlyCreatedGame = games.FirstOrDefault((game) => game.Name == roomName);
                    JoinGame(recentlyCreatedGame.Id);
                });
            });
        }

        OnRoomFinalized?.Invoke(roomName, successfully);
    }

    public void DirectJoin()
    {
        networkManager.StartClient();
    }
    public void CreateRoom()
    {
        string roomName = "SomeRandomName";
        //RoomOptions roomOptions = new RoomOptions { IsPublic = false };
        //Msf.Server.Rooms.RegisterRoom(RoomCreationHandler);
        var spawnOptions = new DictionaryOptions();
        spawnOptions.Add(MsfDictKeys.isPublic, true);
        spawnOptions.Add(MsfDictKeys.roomName, roomName);
        //spawnOptions.Add(MsfDictKeys.maxPlayers, createNewRoomView.MaxConnections);
        //spawnOptions.Add(MsfDictKeys.roomPassword, createNewRoomView.Password);

        // Custom options that will be given to room directly
        var customSpawnOptions = new DictionaryOptions();
        customSpawnOptions.Add(Msf.Args.Names.StartClientConnection, string.Empty); //Seems to mean 'connect spawned server to master'

        Debug.Log("Requesting spawn");
        Msf.Client.Spawners.RequestSpawn(spawnOptions, customSpawnOptions, string.Empty,
        (SpawnRequestController controller, string error) =>
        {
            if (controller == null)
            {
                Debug.LogError("Could not spawn server: " + error);
                return;
            }

            Debug.Log("Waiting for spawn status to be finalized");
            MsfTimer.WaitWhile(() =>
            {
                return controller.Status != SpawnStatus.Finalized;
            }, (isSuccess) =>
            {
                RoomFinalized(roomName, isSuccess);
            }, 60f);
        });
    }

    private void JoinGame(int roomId)
    {
        Debug.Log("Requesting access to room " + roomId);
        Msf.Client.Rooms.GetAccess(roomId, RoomAccessHandler);
    }

    private void SignInHandler(AccountInfoPacket accountInfo, string error)
    {
        if (accountInfo == null)
        {
            Debug.LogError("Could not sign in: " + error);
            return;
        }
    }
    private void RoomCreationHandler(RoomController controller, string error)
    {
        if (controller == null || !controller.IsActive)
        {
            Debug.LogError("Could not create room: " + error);
            return;
        }
        
        //Msf.Client.Rooms.GetAccess(controller.RoomId, RoomAccessHandler);
    }
    private void RoomAccessHandler(RoomAccessPacket accessPacket, string error)
    {
        if (accessPacket == null || string.IsNullOrEmpty(accessPacket.RoomIp))
        {
            Debug.LogError("Could not access room: " + error);
            return;
        }

        Debug.Log("Starting client connection to room");
        networkManager.networkAddress = accessPacket.RoomIp;
        //telepathyTransport.port = (ushort)accessPacket.RoomPort;
        websocketTransport.port = accessPacket.RoomPort;
        networkManager.StartClient();
    }
}
