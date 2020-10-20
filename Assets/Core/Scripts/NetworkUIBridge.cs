using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class ConnectionEvent : UnityEvent<bool> {}

[System.Serializable]
public class RoomEvent : UnityEvent<string, bool> {}

public class NetworkUIBridge : MonoBehaviour
{
    public const string ROOM_NAME_URL_PARAM = "roomid";
    public string gameUrl = "https://oxgames.co/jigether";

    private bool prevMSFConnected;
    private bool prevClientConnected;
    private bool prevMSFSignedIn;

    public bool creatingRoom { get; private set; }
    public bool connectingToRoom { get; private set; }
    public bool connectingToGameServer { get; private set; }
    public bool connectedToGameServer { get; private set; }
    private JigsawStateMachine stateMachine;

    void Awake()
    {
        //SetInteractables(false);
        stateMachine = FindObjectOfType<JigsawStateMachine>();
    }
    void Update()
    {
        // SetConnectingToGameServerValue(NetworkClient.active && !NetworkClient.isConnected);
        // SetConnectedToGameServerValue(NetworkClient.isConnected);
        SetConnectingToGameServerValue(false);
        SetConnectedToGameServerValue(false);
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

    public void AutoConnectUsingUrlParam()
    {
        string roomid = URLReader.GetQueryParam(ROOM_NAME_URL_PARAM);
        // if (!string.IsNullOrEmpty(roomid))
        //     TryJoinRoom(roomid);
    }
    public string GenerateRoomUrl()
    {
        string outputUrl = gameUrl;
        // if (gameInfo != null)
        //     outputUrl += "?" + ROOM_NAME_URL_PARAM + "=" + gameInfo.Name;
        return outputUrl;
    }
    public void CopyUrlToClipboard()
    {
        TextEditor te = new TextEditor();
        te.text = GenerateRoomUrl();
        te.SelectAll();
        te.Copy();
    }
}
