using Barebones.Bridges.Mirror;
using Barebones.MasterServer;
//using Barebones.Networking;
using Mirror;

public class WebSocketMirrorRoomClient : MirrorRoomClient
{
    private int lastRoomIdRequestedAccessTo = int.MinValue;

    /// <summary>
    /// Start room client
    /// </summary>
    public void ConnectToRoom(int roomId)
    {
        if (!Msf.Client.Auth.IsSignedIn)
        {
            logger.Error("Could not connect to room, not signed in");
        }
        else
        {
            // Let's get access to room
            GetRoomAccess(roomId);
        }
    }

    public override void SetPort(int port)
    {
        if (Transport.activeTransport is Mirror.Websocket.WebsocketTransport transport)
        {
            transport.port = (ushort)port;
        }
        else
        {
            logger.Error("You are trying to use WebsocketTransport. But it is not found on the scene. Try to override this method from MirrorRoomClient to create you own implementation");
        }
    }

    public override int GetPort()
    {
        if (Transport.activeTransport is Mirror.Websocket.WebsocketTransport transport)
        {
            return (int)transport.port;
        }
        else
        {
            logger.Error("You are trying to use WebsocketTransport. But it is not found on the scene. Try to override this method from MirrorRoomClient to create you own implementation");
            return 0;
        }
    }

    /// <summary>
    /// Fired when mirror client is started.
    /// </summary>
    protected override void OnMirrorClientStartedEventHandler()
    {
    }
    /// <summary>
    /// Fired when msf user is successfully signed in in test mode
    /// </summary>
    /// <param name="accountInfo"></param>
    /// <param name="error"></param>
    protected override void OnSignInCallbackHandler(AccountInfoPacket accountInfo, string error)
    {
        if (accountInfo == null)
        {
            logger.Error(error);
            return;
        }
    }
}
