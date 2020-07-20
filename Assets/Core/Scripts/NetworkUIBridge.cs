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
    public TMPro.TMP_InputField roomNameField;
    public TMPro.TextMeshProUGUI playersLabel;
    public TMPro.TextMeshProUGUI roomNameLabel;
    public UnityEngine.UI.Selectable[] enabledOnlyIfConnected;

    [Space(10)]
    public GameObject msfScreen;
    public GameObject selfHostScreen;
    public GameObject loadingScreen;
    public GameObject inGameScreen;

    [Space(10)]
    public ConnectionEvent OnMSFConnectionChanged;
    public ConnectionEvent OnClientConnectionChanged;
    public ConnectionEvent OnMSFSignedIn;
    public RoomEvent OnRoomFinalized;

    private bool prevMSFConnected;
    private bool prevClientConnected;
    private bool prevMSFSignedIn;

    private bool creatingRoom;
    private bool connectingToRoom;
    private bool connectingToGameServer;
    private bool connectedToGameServer;
    private GameInfoPacket gameInfo;
    //private string nameOfRoom;
    //private int roomId;

    void Awake()
    {
        SetInteractables(false);
        networkManager = FindObjectOfType<NetworkManager>();
        telepathyTransport = networkManager.GetComponent<TelepathyTransport>();
        websocketTransport = networkManager.GetComponent<Mirror.Websocket.WebsocketTransport>();
    }
    void Update()
    {
        connectingToGameServer = (NetworkClient.active && !NetworkClient.isConnected);
        connectedToGameServer = NetworkClient.isConnected;

        CheckChanges();
        UpdateMenus();
    }

    private void UpdateMenus()
    {
        msfScreen.SetActive(!creatingRoom && !connectingToRoom && !connectingToGameServer && !connectedToGameServer);
        loadingScreen.SetActive(creatingRoom || connectingToRoom || connectingToGameServer);
        inGameScreen.SetActive(connectedToGameServer);
        playersLabel.text = "Players: " + gameInfo.OnlinePlayers + "/" + gameInfo.MaxPlayers;
        roomNameLabel.text = "Room code: " + gameInfo.Name;
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
            TryJoinRoom(roomName);
        }

        OnRoomFinalized?.Invoke(roomName, successfully);
    }

    public void DirectJoin()
    {
        networkManager.StartClient();
    }
    public void CreateRoom()
    {
        creatingRoom = true;
        string roomName = System.DateTime.Now.Ticks.ToString();
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
                creatingRoom = false;
                Debug.LogError("Could not spawn server: " + error);
                return;
            }

            Debug.Log("Waiting for spawn status to be finalized");
            MsfTimer.WaitWhile(() =>
            {
                return controller.Status != SpawnStatus.Finalized;
            }, (isSuccess) =>
            {
                creatingRoom = false;
                RoomFinalized(roomName, isSuccess);
            }, 60f);
        });
    }

    public void JoinGame()
    {
        var roomName = roomNameField.text;
        TryJoinRoom(roomName);
    }
    private void JoinGame(int roomId)
    {
        Debug.Log("Requesting access to room " + roomId);
        Msf.Client.Rooms.GetAccess(roomId, RoomAccessHandler);
    }
    private void TryJoinRoom(string roomName)
    {
        Debug.Log("Requesting games");
        connectingToRoom = true;
        MsfTimer.WaitForSeconds(0.2f, () =>
        {
            Msf.Client.Matchmaker.FindGames((games) =>
            {
                Debug.Log("Received " + games.Count() + " game(s)");
                var foundGame = games.FirstOrDefault((game) => game.Name == roomName);
                if (foundGame != null)
                {
                    //nameOfRoom = roomName;
                    //roomId = foundGame.Id;
                    gameInfo = foundGame;
                    JoinGame(foundGame.Id);
                }
                else
                {
                    connectingToRoom = false;
                    Debug.LogError("Could not find room with name: " + roomName);
                }
            });
        });
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
        connectingToRoom = false;
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
