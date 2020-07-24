using Barebones.Bridges.Mirror;
using UnityEngine;

public class JigsawNetworkManager : MirrorNetworkManager
{
    public GameObject jigsawGamePrefab;

    public override void Awake()
    {
        base.Awake();
        autoCreatePlayer = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        var jigsawGame = Instantiate(jigsawGamePrefab, Vector3.zero, Quaternion.identity);
        Mirror.NetworkServer.Spawn(jigsawGame);
        jigsawGame.GetComponent<JigsawGame>().LoadJigsawPuzzle();
    }
}
