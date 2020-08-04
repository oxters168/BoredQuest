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
    public const string ROOM_NAME_URL_PARAM = "roomid";
    public string gameUrl = "https://oxgames.co/jigether";
    private NetworkManager networkManager;
    private TelepathyTransport telepathyTransport;
    private Mirror.Websocket.WebsocketTransport websocketTransport;

    [Space(10)]
    public ConnectionEvent OnMSFConnectionChanged;
    public ConnectionEvent OnClientConnectionChanged;
    public ConnectionEvent OnMSFSignedIn;
    public RoomEvent OnRoomFinalized;

    private bool prevMSFConnected;
    private bool prevClientConnected;
    private bool prevMSFSignedIn;

    public bool creatingRoom { get; private set; }
    public bool connectingToRoom { get; private set; }
    public bool connectingToGameServer { get; private set; }
    public bool connectedToGameServer { get; private set; }
    public GameInfoPacket gameInfo;// { get; private set; }
    private JigsawStateMachine stateMachine;

    void Awake()
    {
        //SetInteractables(false);
        networkManager = FindObjectOfType<NetworkManager>();
        stateMachine = FindObjectOfType<JigsawStateMachine>();
        telepathyTransport = networkManager.GetComponent<TelepathyTransport>();
        websocketTransport = networkManager.GetComponent<Mirror.Websocket.WebsocketTransport>();
    }
    void Update()
    {
        SetConnectingToGameServerValue(NetworkClient.active && !NetworkClient.isConnected);
        SetConnectedToGameServerValue(NetworkClient.isConnected);

        CheckChanges();
        //UpdateMenus();
        //UpdateOnScreenInfo();
    }

    private void OnConnectedToGameServerValueChanged()
    {
        //UpdateMenus();
        //stateMachine.SetState(connectedToGameServer ? JigsawStateMachine.GameState.gameSetup : JigsawStateMachine.GameState.none);
    }
    private void SetConnectedToGameServerValue(bool onOff)
    {
        var prevValue = connectedToGameServer;
        connectedToGameServer = onOff;
        if (prevValue != onOff)
            OnConnectedToGameServerValueChanged();
    }
    private void OnConnectingToGameServerValueChanged()
    {
        //UpdateMenus();
    }
    private void SetConnectingToGameServerValue(bool onOff)
    {
        var prevValue = connectingToGameServer;
        connectingToGameServer = onOff;
        if (prevValue != onOff)
            OnConnectingToGameServerValueChanged();
    }
    private void OnConnectingToRoomValueChanged()
    {
        //UpdateMenus();
    }
    private void SetConnectingToRoomValue(bool onOff)
    {
        var prevValue = connectingToRoom;
        connectingToRoom = onOff;
        if (prevValue != onOff)
            OnConnectingToRoomValueChanged();
    }
    private void OnCreatingRoomValueChanged()
    {
        //UpdateMenus();
    }
    private void SetCreatingRoomValue(bool onOff)
    {
        var prevValue = creatingRoom;
        creatingRoom = onOff;
        if (prevValue != onOff)
            OnCreatingRoomValueChanged();
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
        //SetInteractables(isConnected);
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
        #if UNITY_WEBGL
        AutoConnectUsingUrlParam();
        #endif

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

    public void AutoConnectUsingUrlParam()
    {
        string roomid = URLReader.GetQueryParam(ROOM_NAME_URL_PARAM);
        if (!string.IsNullOrEmpty(roomid))
            TryJoinRoom(roomid);
    }
    public string GenerateRoomUrl()
    {
        string outputUrl = gameUrl;
        if (gameInfo != null)
            outputUrl += "?" + ROOM_NAME_URL_PARAM + "=" + gameInfo.Name;
        return outputUrl;
    }
    public void CopyUrlToClipboard()
    {
        TextEditor te = new TextEditor();
        te.text = GenerateRoomUrl();
        te.SelectAll();
        te.Copy();
    }

    public void DirectJoin()
    {
        //networkManager.networkAddress = "178.62.6.45";
        networkManager.networkAddress = "127.0.0.1";
        websocketTransport.port = 7778;
        networkManager.StartClient();
    }
    public void CreateRoom()
    {
        SetCreatingRoomValue(true);
        //string roomName = System.DateTime.Now.Ticks.ToString();
        string roomName = UnityHelpers.GuidHelpers.Encode(System.Guid.NewGuid());
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
                SetCreatingRoomValue(false);
                Debug.LogError("Could not spawn server: " + error);
                return;
            }

            Debug.Log("Waiting for spawn status to be finalized");
            MsfTimer.WaitWhile(() =>
            {
                return controller.Status != SpawnStatus.Finalized;
            }, (isSuccess) =>
            {
                SetCreatingRoomValue(false);
                RoomFinalized(roomName, isSuccess);
            }, 60f);
        });
    }

    public void JoinGame()
    {
        var roomName = FindObjectOfType<MenuManager>().roomNameField.text;
        //var roomName = roomNameField.text;
        TryJoinRoom(roomName);
    }
    private void JoinGame(int roomId)
    {
        Debug.Log("Requesting access to room " + roomId);
        SetConnectingToRoomValue(false);
        //MsfDictKeys.roomId = roomId.ToString();
        //Barebones.Bridges.Mirror.MirrorRoomClient.Instance.StartClient();
        ((WebSocketMirrorRoomClient)Barebones.Bridges.Mirror.MirrorRoomClient.Instance).ConnectToRoom(roomId);
    }
    private void TryJoinRoom(string roomName)
    {
        Debug.Log("Requesting games");
        SetConnectingToRoomValue(true);
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
                    SetConnectingToRoomValue(false);
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
        SetConnectingToRoomValue(false);
        /*if (accessPacket == null || string.IsNullOrEmpty(accessPacket.RoomIp))
        {
            Debug.LogError("Could not access room: " + error);
            return;
        }

        Debug.Log("Starting client connection to room");
        networkManager.networkAddress = accessPacket.RoomIp;
        //telepathyTransport.port = (ushort)accessPacket.RoomPort;
        websocketTransport.port = accessPacket.RoomPort;
        networkManager.StartClient();*/
    }
}
