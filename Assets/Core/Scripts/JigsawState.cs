using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityHelpers;

[Serializable]
public class JigsawState
{
    public List<ClusterWrapper> clusters = new List<ClusterWrapper>();
    public bool containsPuzzleSize;
    public Vector3 puzzleSize;
    public bool containsPieceCount;
    public Vector2Int puzzlePieceCount;
    public bool containsBoundaryPercent;
    public Vector2 pieceBoundaryPercent;
    public bool containsSeed;
    public int seed;
    public bool containsIsLoading;
    public bool isLoading;
    public bool containsIsLoaded;
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
            if (currentState.containsPuzzleSize)
                jigsawGame.puzzleSize = currentState.puzzleSize;
            if (currentState.containsPieceCount)
                jigsawGame.puzzlePieceCount = currentState.puzzlePieceCount;
            if (currentState.containsBoundaryPercent)
                jigsawGame.pieceBoundaryPercent = currentState.pieceBoundaryPercent;
            if (currentState.containsSeed)
                jigsawGame.seed = currentState.seed;
            if (!jigsawGame.isLoaded && !jigsawGame.isLoading && ((currentState.containsIsLoaded && currentState.isLoaded) || (currentState.containsIsLoading && currentState.isLoading)))
                jigsawGame.LoadJigsawPuzzle();
            else if ((jigsawGame.isLoaded || jigsawGame.isLoading) && (currentState.containsIsLoaded && !currentState.isLoaded) && (currentState.containsIsLoading && !currentState.isLoading))
                jigsawGame.DestroyJigsawPuzzle();

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
                            if (cluster.containsTransform)
                            {
                                clusterParent.position = cluster.position;
                                clusterParent.rotation = cluster.rotation;
                            }
                            if (cluster.containsIndices)
                            {
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
    }
    /*public static void Serialize(NetworkWriter writer, JigsawState previousState, JigsawState currentState, float changeTolerance = 0.01f)
    {
        var writePuzzleSize = previousState != null ? !previousState.puzzleSize.EqualTo(currentState.puzzleSize, changeTolerance) : true;
        writer.WriteBoolean(writePuzzleSize);
        if (writePuzzleSize)
            writer.WriteVector3(currentState.puzzleSize);

        var writePieceCount = previousState != null ? (!previousState.puzzlePieceCount.EqualTo(currentState.puzzlePieceCount)) : true;
        writer.WriteBoolean(writePieceCount);
        if (writePieceCount)
            writer.WriteVector2Int(currentState.puzzlePieceCount);

        var writeBoundaryPercent = previousState != null ? (!previousState.pieceBoundaryPercent.EqualTo(currentState.pieceBoundaryPercent, changeTolerance)) : true;
        writer.WriteBoolean(writeBoundaryPercent);
        if (writeBoundaryPercent)
            writer.WriteVector2(currentState.pieceBoundaryPercent);

        var writeSeed = previousState != null ? (previousState.seed != currentState.seed) : true;
        writer.WriteBoolean(writeSeed);
        if (writeSeed)
            writer.WriteInt32(currentState.seed);

        var writeIsLoading = previousState != null ? (previousState.isLoading != currentState.isLoading) : true;
        writer.WriteBoolean(writeIsLoading);
        if (writeIsLoading)
            writer.WriteBoolean(currentState.isLoading);

        var writeIsLoaded = previousState != null ? (previousState.isLoaded != currentState.isLoaded) : true;
        writer.WriteBoolean(writeIsLoaded);
        if (writeIsLoaded)
            writer.WriteBoolean(currentState.isLoaded);

        var clusters = currentState.clusters;
        if (clusters != null)
        {
            //var pieces = currentState.pieces;
            foreach (var cluster in clusters)
            {
                if (cluster.indices.Count > 0)
                {
                    var firstIndex = cluster.indices[0];
                    IEnumerable<ClusterWrapper> searchResults = null;
                    if (previousState != null)
                        searchResults = previousState.clusters.Where((c) => c.indices.Contains(firstIndex));
                    ClusterWrapper previousCluster = (searchResults != null && searchResults.Count() > 0) ? searchResults.First() : null;

                    var writeClusterTransform = previousCluster != null ? (!previousCluster.position.EqualTo(cluster.position, changeTolerance)) || (!previousCluster.rotation.EqualTo(cluster.rotation, changeTolerance)) : true;
                    var writeClusterIndices = previousCluster != null ? previousCluster.indices.Count != cluster.indices.Count : true;
                    writer.WriteBoolean(writeClusterIndices);
                    if (writeClusterIndices)
                    {
                        foreach (var pieceInCluster in cluster.indices)
                            writer.WriteInt32(pieceInCluster);
                    }
                    else
                        writer.WriteInt32(firstIndex);
                    writer.WriteInt32(-1);

                    writer.WriteBoolean(writeClusterTransform);
                    if (writeClusterTransform)
                    {
                        writer.WriteVector3(cluster.position);
                        writer.WriteQuaternion(cluster.rotation);
                    }
                }
            }
        }
        writer.WriteBoolean(false); //For the last 'contains indices'
        writer.WriteInt32(int.MinValue);
    }
    public static JigsawState Deserialize(NetworkReader reader)
    {
        JigsawState deserializedState = new JigsawState();

        deserializedState.containsPuzzleSize = reader.ReadBoolean();
        if (deserializedState.containsPuzzleSize)
            deserializedState.puzzleSize = reader.ReadVector3();

        deserializedState.containsPieceCount = reader.ReadBoolean();
        if (deserializedState.containsPieceCount)
            deserializedState.puzzlePieceCount = reader.ReadVector2Int();

        deserializedState.containsBoundaryPercent = reader.ReadBoolean();
        if (deserializedState.containsBoundaryPercent)
            deserializedState.pieceBoundaryPercent = reader.ReadVector2();

        deserializedState.containsSeed = reader.ReadBoolean();
        if (deserializedState.containsSeed)
            deserializedState.seed = reader.ReadInt32();

        deserializedState.containsIsLoading = reader.ReadBoolean();
        if (deserializedState.containsIsLoading)
            deserializedState.isLoading = reader.ReadBoolean();

        deserializedState.containsIsLoaded = reader.ReadBoolean();
        if (deserializedState.containsIsLoaded)
            deserializedState.isLoaded = reader.ReadBoolean();
        
        List<ClusterWrapper> clusters = new List<ClusterWrapper>();
        int currentIndex = int.MaxValue;
        ClusterWrapper currentCluster = new ClusterWrapper();
        while (currentIndex != int.MinValue)
        {
            if (currentIndex < 0)
            {
                currentCluster.containsTransform = reader.ReadBoolean();
                if (currentCluster.containsTransform)
                {
                    currentCluster.position = reader.ReadVector3();
                    currentCluster.rotation = reader.ReadQuaternion();
                }
                clusters.Add(currentCluster);
                currentCluster = new ClusterWrapper();
                currentIndex = int.MaxValue;
            }
            else if (currentIndex != int.MaxValue)
                currentCluster.indices.Add(currentIndex);
            
            if (currentIndex == int.MaxValue)
                currentCluster.containsIndices = reader.ReadBoolean();
            
            currentIndex = reader.ReadInt32();
        }
        deserializedState.clusters = clusters;

        return deserializedState;
    }*/

    [Serializable]
    public class ClusterWrapper
    {
        public bool containsTransform;
        public Vector3 position;
        public Quaternion rotation;
        public bool containsIndices;
        public List<int> indices = new List<int>();
    }
}
