using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[Serializable]
public class JigsawState
{
    public List<ClusterWrapper> clusters = new List<ClusterWrapper>();
    public Vector3 puzzleSize;
    public Vector2Int puzzlePieceCount;
    public Vector2 pieceBoundaryPercent;
    public int seed;
    public bool isLoading;
    public bool isLoaded;

    public static JigsawState GetCurrentState(JigsawGame game)
    {
        var wrapped = new JigsawState();
        wrapped.puzzleSize = game.puzzleSize;
        wrapped.puzzlePieceCount = game.puzzlePieceCount;
        wrapped.pieceBoundaryPercent = game.pieceBoundaryPercent;
        wrapped.seed = game.seed;
        wrapped.isLoading = game.isLoading;
        wrapped.isLoaded = game.isLoaded;

        var clusters = game.clusters;
        if (clusters != null)
        {
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

    public static void ApplyToGame(JigsawGame jigsawGame, JigsawState currentState)
    {
        if (currentState != null)
        {
            jigsawGame.puzzleSize = currentState.puzzleSize;
            jigsawGame.puzzlePieceCount = currentState.puzzlePieceCount;
            jigsawGame.pieceBoundaryPercent = currentState.pieceBoundaryPercent;
            jigsawGame.seed = currentState.seed;
            if (!jigsawGame.isLoaded && !jigsawGame.isLoading && (currentState.isLoaded || currentState.isLoading))
                jigsawGame.LoadJigsawPuzzle();

            if (jigsawGame.pieces != null && currentState.clusters.Count > 0)
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
            }
        }
    }
    public static void Serialize(NetworkWriter writer, JigsawState currentState)
    {
        writer.WriteVector3(currentState.puzzleSize);
        writer.WriteVector2Int(currentState.puzzlePieceCount);
        writer.WriteVector2(currentState.pieceBoundaryPercent);
        writer.WriteInt32(currentState.seed);
        writer.WriteBoolean(currentState.isLoading);
        writer.WriteBoolean(currentState.isLoaded);

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
    public static JigsawState Deserialize(NetworkReader reader)
    {
        Vector3 puzzleSize = reader.ReadVector3();
        Vector2Int puzzlePieceCount = reader.ReadVector2Int();
        Vector2 pieceBoundaryPercent = reader.ReadVector2();
        int seed = reader.ReadInt32();
        bool isLoading = reader.ReadBoolean();
        bool isLoaded = reader.ReadBoolean();
        
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

        return new JigsawState()
        {
            puzzleSize = puzzleSize,
            puzzlePieceCount = puzzlePieceCount,
            pieceBoundaryPercent = pieceBoundaryPercent,
            seed = seed, clusters = clusters,
            isLoading = isLoading,
            isLoaded = isLoaded
        };
    }

    [Serializable]
    public class ClusterWrapper
    {
        public Vector3 position;
        public Quaternion rotation;
        public List<int> indices = new List<int>();
    }
}
