using Barebones.Bridges.Mirror;

public class JigsawNetworkManager : MirrorNetworkManager
{
    public override void Awake()
    {
        base.Awake();
        autoCreatePlayer = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    public override void OnClientConnect(Mirror.NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        
    }
}
