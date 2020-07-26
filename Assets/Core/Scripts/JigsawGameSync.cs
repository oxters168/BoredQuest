using UnityEngine;
using Mirror;
using System.ComponentModel;

public class JigsawGameSync : NetworkBehaviour
{
    [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    public bool clientAuthority;
    private float lastClientSendTime = float.MinValue;
    // Is this a client with authority over this transform?
    // This component could be on the player object or any object that has been assigned authority to this client.
    bool IsClientWithAuthority => hasAuthority && clientAuthority;
    public float changeTolerance = 0.01f;

    private JigsawGame jigsawGame { get { if (_jigsawGame == null) _jigsawGame = GetComponent<JigsawGame>(); return _jigsawGame; } }
    private JigsawGame _jigsawGame;

    private JigsawState currentState;

    void Update()
    {
        // if server then always sync to others.
        if (isServer)
        {
            // just use OnSerialize via SetDirtyBit only sync when position
            // changed. set dirty bits 0 or 1
            SetDirtyBit(HasChanged() ? 1UL : 0UL);
        }

        // no 'else if' since host mode would be both
        if (isClient)
        {
            // send to server if we have local authority (and aren't the server)
            // -> only if connectionToServer has been initialized yet too
            if (!isServer && IsClientWithAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {
                    if (HasChanged())
                    {
                        // serialize
                        // local position/rotation for VR support
                        using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                        {
                            SerializeIntoWriter(writer, JigsawState.GetCurrentState(jigsawGame));

                            // send to server
                            CmdClientToServerSync(writer.ToArray());
                        }
                    }
                    lastClientSendTime = Time.time;
                }
            }

            // apply interpolation on client for all players
            // unless this client has authority over the object. could be
            // himself or another object that he was assigned authority over
            if (!IsClientWithAuthority)
            {
                ApplyValues();
            }
        }
    }

    // moved since last time we checked it?
    bool HasChanged()
    {
        bool changed = false;
        if (jigsawGame.pieces != null)
        {
            if (currentState != null)
            {
                foreach(var cluster in currentState.clusters)
                {
                    if (cluster.indices.Count > 0)
                    {
                        var clusterParent = jigsawGame.GetPieceKey(cluster.indices[0]);
                        var indicesChanged = jigsawGame.clusters[clusterParent].Count != cluster.indices.Count;
                        var clusterMoved = Vector3.Distance(clusterParent.position, cluster.position) > changeTolerance;
                        var clusterRotated = Quaternion.Angle(clusterParent.rotation, cluster.rotation) > changeTolerance;

                        changed = indicesChanged || clusterMoved || clusterRotated;
                        if (changed)
                            break;
                    }
                }
            }
            else
                changed = true;
        }

        return changed;
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        SerializeIntoWriter(writer, JigsawState.GetCurrentState(jigsawGame));
        return true;
    }
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        DeserializeFromReader(reader);
    }

    // serialization is needed by OnSerialize and by manual sending from authority
    // public only for tests
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SerializeIntoWriter(NetworkWriter writer, JigsawState currentState)
    {
        // serialize position, rotation, scale
        // note: we do NOT compress rotation.
        //       we are CPU constrained, not bandwidth constrained.
        //       the code needs to WORK for the next 5-10 years of development.
        JigsawState.Serialize(writer, currentState);
    }
    private void DeserializeFromReader(NetworkReader reader)
    {
        currentState = JigsawState.Deserialize(reader);
    }
    private void ApplyValues()
    {
        JigsawState.ApplyToGame(jigsawGame, currentState);
    }

    [Command(ignoreAuthority = true)]
    public void CmdSetColumnsValue(int columns)
    {
        //var jigsawGame = FindObjectOfType<JigsawGame>();
        jigsawGame.puzzlePieceCount = new Vector2Int(columns, jigsawGame.puzzlePieceCount.y);
    }
    [Command(ignoreAuthority = true)]
    public void CmdSetRowsValue(int rows)
    {
        //var jigsawGame = FindObjectOfType<JigsawGame>();
        jigsawGame.puzzlePieceCount = new Vector2Int(jigsawGame.puzzlePieceCount.x, rows);
    }
    [Command(ignoreAuthority = true)]
    public void CmdLoadJigsawPuzzle()
    {
        jigsawGame.LoadJigsawPuzzle();
    }

    // local authority client sends sync message to server for broadcasting
    [Command]
    void CmdClientToServerSync(byte[] payload)
    {
        // Ignore messages from client if not in client authority mode
        if (!clientAuthority)
            return;

        // deserialize payload
        using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(payload))
            DeserializeFromReader(networkReader);

        // server-only mode does no interpolation to save computations,
        // but let's set the position directly
        if (isServer && !isClient)
            ApplyValues();

        // set dirty so that OnSerialize broadcasts it
        SetDirtyBit(1UL);
    }
}
