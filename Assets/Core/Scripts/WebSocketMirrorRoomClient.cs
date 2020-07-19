using Barebones.Bridges.Mirror;
using Mirror;

public class WebSocketMirrorRoomClient : MirrorRoomClient
{
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
}
