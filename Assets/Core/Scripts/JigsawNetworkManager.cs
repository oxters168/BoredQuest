using UnityEngine;

public class JigsawNetworkManager : MonoBehaviour//MirrorNetworkManager
{
    /*public GameObject jigsawGamePrefab;

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
        
        var gameScript = jigsawGame.GetComponent<JigsawGame>();
        gameScript.seed = Random.Range(1337, 42070);
        //var pieceCount = Random.Range(2, 13);
        //gameScript.puzzlePieceCount = new Vector2Int(pieceCount, pieceCount);
        //gameScript.LoadJigsawPuzzle();
    }*/
}
