using UnityEngine;
using Mirror;
using System.ComponentModel;
using System.Collections.Generic;

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

    private JigsawGameWrapper currentState;

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
                            SerializeIntoWriter(writer, JigsawGameWrapper.FromJigsawGame(jigsawGame));

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
        //var horizontal = Movement.GetAxis("Horizontal");
        //var vertical = Movement.GetAxis("Vertical");
        //var grab = Movement.GetToggle("Grab");

        // moved or rotated or scaled?
        // local position/rotation/scale for VR support
        //bool horizontalChanged = Mathf.Abs(prevHorizontal - horizontal) > float.Epsilon;
        //bool verticalChanged = Mathf.Abs(prevVertical - vertical) > float.Epsilon;
        //bool changes = horizontalChanged || verticalChanged || (grab != prevGrab);

        // save last for next frame to compare
        // (only if change was detected. otherwise slow moving objects might
        //  never sync because of C#'s float comparison tolerance. see also:
        //  https://github.com/vis2k/Mirror/pull/428)
        if (changed)
        {
            // local position/rotation for VR support
            //prevHorizontal = horizontal;
            //prevVertical = vertical;
            //prevGrab = grab;
        }
        return changed;
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        SerializeIntoWriter(writer, JigsawGameWrapper.FromJigsawGame(jigsawGame));
        return true;
    }
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        DeserializeFromReader(reader);
    }

    // serialization is needed by OnSerialize and by manual sending from authority
    // public only for tests
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SerializeIntoWriter(NetworkWriter writer, JigsawGameWrapper currentState)
    {
        // serialize position, rotation, scale
        // note: we do NOT compress rotation.
        //       we are CPU constrained, not bandwidth constrained.
        //       the code needs to WORK for the next 5-10 years of development.
        var clusters = currentState.clusters;
        if (clusters != null)
        {
            //var pieces = currentState.pieces;
            foreach (var cluster in clusters)
            {
                foreach (var pieceInCluster in cluster.indices)
                {
                    //var pieceIndex = System.Array.IndexOf(pieces, pieceInCluster);
                    writer.WriteInt32(pieceInCluster);
                }
                writer.WriteInt32(-1);
                writer.WriteVector3(cluster.position);
                writer.WriteQuaternion(cluster.rotation);
            }
        }
        writer.WriteInt32(int.MinValue);
    }
    private void DeserializeFromReader(NetworkReader reader)
    {
        List<ClusterWrapper> clusters = new List<ClusterWrapper>();
        int currentIndex = int.MaxValue;
        ClusterWrapper currentCluster = new ClusterWrapper();
        while (currentIndex != int.MinValue)
        {
            if (currentIndex < 0)
            {
                currentCluster.position = reader.ReadVector3();
                currentCluster.rotation = reader.ReadQuaternion();
                clusters.Add(currentCluster);
                currentCluster = new ClusterWrapper();
            }
            else if (currentIndex != int.MaxValue)
                currentCluster.indices.Add(currentIndex);
            
            currentIndex = reader.ReadInt32();
        }

        currentState = new JigsawGameWrapper() { clusters = clusters };
    }
    private void ApplyValues()
    {
        if (currentState != null && jigsawGame.pieces != null && currentState.clusters.Count > 0)
        {
            int columns = jigsawGame.puzzlePieceCount.x;
            int rows = jigsawGame.puzzlePieceCount.y;
            float pieceWidth = jigsawGame.puzzleSize.x / columns;
            float pieceHeight = jigsawGame.puzzleSize.z / rows;

            foreach (var cluster in currentState.clusters)
            {
                if (cluster.indices.Count > 0)
                {
                    var firstIndex = cluster.indices[0];
                    int mainColIndex = firstIndex % columns;
                    int mainRowIndex = firstIndex / columns;

                    var clusterParent = jigsawGame.GetPieceKey(firstIndex);
                    if (clusterParent != null)
                    {
                        clusterParent.position = cluster.position;
                        clusterParent.rotation = cluster.rotation;
                        foreach (var pieceIndex in cluster.indices)
                        {
                            var pieceParent = jigsawGame.GetPieceKey(pieceIndex);
                            if (pieceParent != clusterParent)
                            {
                                int currentColIndex = pieceIndex % columns;
                                int currentRowIndex = pieceIndex / columns;
                                int horOffset = currentColIndex - mainColIndex;
                                int verOffset = currentRowIndex - mainRowIndex;

                                var pieceObject = jigsawGame.pieces[pieceIndex].transform;
                                pieceObject.SetParent(clusterParent);
                                pieceObject.localPosition = new Vector3(horOffset * pieceWidth, 0, verOffset * pieceHeight);

                                jigsawGame.clusters[pieceParent].Remove(pieceIndex);
                                jigsawGame.clusters[clusterParent].Add(pieceIndex);
                            }
                        }
                    }
                }
            }
            /*int currentIndex = 0;
            foreach (var clusterPair in jigsawGame.clusters)
            {
                var currentCluster = currentState.clusters[currentIndex++];
                clusterPair.Key.position = currentCluster.position;
                clusterPair.Key.rotation = currentCluster.rotation;

                clusterPair.Value.Clear();
                for (int i = 0; i < currentCluster.indices.Count; i++)
                {
                    var pieceIndex = currentCluster.indices[i];
                    var currentPiece = jigsawGame.pieces[pieceIndex];
                    if (i == 0)
                        currentPiece.transform.SetParent(clusterPair.Key, false);
                    clusterPair.Value.Add(pieceIndex);
                }
            }*/
        }
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

    public class JigsawGameWrapper
    {
        public List<ClusterWrapper> clusters = new List<ClusterWrapper>();

        public static JigsawGameWrapper FromJigsawGame(JigsawGame game)
        {
            var wrapped = new JigsawGameWrapper();

            var clusters = game.clusters;
            if (clusters != null)
            {
                //var pieces = currentState.pieces;
                foreach (var cluster in clusters)
                {
                    wrapped.clusters.Add(new ClusterWrapper()
                    {
                        position = cluster.Key.position,
                        rotation = cluster.Key.rotation,
                        indices = cluster.Value
                    });
                }
            }

            return wrapped;
        }
    }
    public class ClusterWrapper
    {
        public Vector3 position;
        public Quaternion rotation;
        public List<int> indices = new List<int>();
    }
}
