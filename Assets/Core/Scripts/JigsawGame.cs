using UnityEngine;
using UnityHelpers;
using System.Collections.Generic;
using System.Linq;

public class JigsawGame : MonoBehaviour
{
    public Dictionary<Transform, List<int>> clusters = new Dictionary<Transform, List<int>>();
    public Vector3 puzzleSize = new Vector3(1, 0.1f, 1);
    public Vector2Int puzzlePieceCount = new Vector2Int(5, 5);
    public Vector2 pieceBoundaryPercent = new Vector2(0.1f, 0.1f);
    public Vector2 boardOuterBorder = Vector2.one;
    public Vector2 boardInnerBorder = new Vector2(0.66f, 0.66f);
    public int seed = 1337;
    public float percentLoaded;
    public bool isLoading { get; private set; }
    public bool isLoaded { get; private set; }

    [Space(10)]
    public Material puzzleFaceMat;
    public Material puzzleSideMat;
    public Material puzzleBackMat;

    [Space(10)]
    public UnityEngine.Events.UnityEvent OnPuzzleLoaded;

    private Transform jigsawParent;
    public JigPieceBehaviour[] pieces;
    private GameObject[] generatedPieces;

    void Awake()
    {
        jigsawParent = new GameObject().transform;
        jigsawParent.name = "JigsawParent";
    }
    //void Start()
    //{
    //    LoadJigsawPuzzle();
    //}

    public void SetPuzzleVisibility(bool onOff)
    {
        jigsawParent.gameObject.SetActive(onOff);
    }
    public void LoadJigsawPuzzle()
    {
        isLoading = true;
        generatedPieces = new GameObject[puzzlePieceCount.x * puzzlePieceCount.y];
        StartCoroutine(JigsawPuzzle.Generate(puzzlePieceCount.x, puzzlePieceCount.y, puzzleSize.x, puzzleSize.z, puzzleSize.y, 5, seed, jigsawParent, generatedPieces, puzzleFaceMat, puzzleSideMat, puzzleBackMat, true, OnPuzzleLoadingPercent, OnPuzzleLoadCompleted));
    }
    public void DestroyJigsawPuzzle()
    {
        percentLoaded = 0;
        isLoaded = false;
        isLoading = false;

        if (pieces != null)
            for (int i = 0; i < pieces.Length; i++)
                Destroy(pieces[i].gameObject);

        foreach (var keyPair in clusters)
            Destroy(keyPair.Key.gameObject);

        clusters.Clear();
        pieces = null;
        generatedPieces = null;
    }

    private void OnAttachAttempt(JigBoundaryCollider sender, JigBoundaryCollider other)
    {
        int senderPieceIndex = System.Array.IndexOf(pieces, sender.jigPiece);
        int otherPieceIndex = System.Array.IndexOf(pieces, other.jigPiece);
        var relation = GetRelation(senderPieceIndex, otherPieceIndex);

        if (relation != JigBoundaryCollider.BoundarySide.none && relation == sender.boundarySide)
        {
            Vector3 firstPieceExpectedRelativePosition, secondPieceExpectedRelativePosition;
            CalculateExpectedRelativePositions(sender.jigPiece, other.jigPiece, relation, out firstPieceExpectedRelativePosition, out secondPieceExpectedRelativePosition);
            
            var firstKey = GetPieceKey(senderPieceIndex);
            var secondKey = GetPieceKey(otherPieceIndex);
            if (firstKey == null)
            {
                if (secondKey == null)
                {
                    //Create new cluster with first and second piece
                    var clusterKey = CreateNewCluster();
                    clusterKey.position = sender.jigPiece.transform.position;
                    other.jigPiece.transform.position = secondPieceExpectedRelativePosition;

                    AddPieceToCluster(clusterKey, senderPieceIndex);
                    AddPieceToCluster(clusterKey, otherPieceIndex);
                }
                else
                {
                    //Add first piece to second's cluster
                    sender.jigPiece.transform.position = firstPieceExpectedRelativePosition;
                    AddPieceToCluster(secondKey, senderPieceIndex);
                }
            }
            else if (secondKey == null)
            {
                //Add second piece to first's cluster
                other.jigPiece.transform.position = secondPieceExpectedRelativePosition;
                AddPieceToCluster(firstKey, otherPieceIndex);
            }
            else if (firstKey != secondKey)
            {
                //Combine clusters
                Vector3 offset = secondPieceExpectedRelativePosition - other.jigPiece.transform.position;
                secondKey.position += offset;

                var secondClusterPieces = clusters[secondKey];
                foreach (var piece in secondClusterPieces)
                    AddPieceToCluster(firstKey, piece);
                clusters.Remove(secondKey);
                Destroy(secondKey.gameObject);
            }
            //other.jigPiece.transform.SetParent(sender.jigPiece.transform);
        }
    }
    private void CalculateExpectedRelativePositions(JigPieceBehaviour firstPiece, JigPieceBehaviour secondPiece, JigBoundaryCollider.BoundarySide relation, out Vector3 firstPieceExpectedRelativePosition, out Vector3 secondPieceExpectedRelativePosition)
    {
        int columns = puzzlePieceCount.x;
        int rows = puzzlePieceCount.y;
        float pieceWidth = puzzleSize.x / columns;
        float pieceHeight = puzzleSize.z / rows;
        firstPieceExpectedRelativePosition = secondPiece.transform.position;
        secondPieceExpectedRelativePosition = firstPiece.transform.position;
        if (relation == JigBoundaryCollider.BoundarySide.top)
        {
            firstPieceExpectedRelativePosition -= Vector3.forward * pieceHeight;
            secondPieceExpectedRelativePosition += Vector3.forward * pieceHeight;
        }
        else if (relation == JigBoundaryCollider.BoundarySide.bottom)
        {
            firstPieceExpectedRelativePosition += Vector3.forward * pieceHeight;
            secondPieceExpectedRelativePosition -= Vector3.forward * pieceHeight;
        }
        else if (relation == JigBoundaryCollider.BoundarySide.left)
        {
            firstPieceExpectedRelativePosition += Vector3.right * pieceWidth;
            secondPieceExpectedRelativePosition -= Vector3.right * pieceWidth;
        }
        else if (relation == JigBoundaryCollider.BoundarySide.right)
        {
            firstPieceExpectedRelativePosition -= Vector3.right * pieceWidth;
            secondPieceExpectedRelativePosition += Vector3.right * pieceWidth;
        }
    }
    public Transform GetPieceKey(int pieceIndex)
    {
        Transform key = null;
        //KeyValuePair<Transform, GameObject[]> defaultValue = default(KeyValuePair<Transform, GameObject[]>);
        var cluster = clusters.FirstOrDefault(pair => pair.Value.IndexOf(pieceIndex) > -1);
        if (!cluster.Equals(default))
            key = cluster.Key;
        return key;
    }
    private void AddPieceToCluster(Transform clusterKey, int pieceIndex)
    {
        //piece.GetComponent<GrabbableBase>().enabled = false;
        var piece = pieces[pieceIndex];
        Destroy(piece.GetComponent<GrabbableBase>());
        piece.transform.SetParent(clusterKey);
        clusters[clusterKey].Add(pieceIndex);
    }
    private Transform CreateNewCluster()
    {
        GameObject clusterParent = new GameObject();
        var rigidbody = clusterParent.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        clusterParent.AddComponent<GrabbableTransform>();
        clusterParent.name = "Cluster";
        clusterParent.transform.SetParent(transform);
        List<int> emptyList = new List<int>();
        clusters.Add(clusterParent.transform, emptyList);
        return clusterParent.transform;
    }
    private JigBoundaryCollider.BoundarySide GetRelation(int firstPieceIndex, int secondPieceIndex)
    {
        var relation = JigBoundaryCollider.BoundarySide.none;

        int columns = puzzlePieceCount.x;
        int diff = secondPieceIndex - firstPieceIndex;
        if (Mathf.Abs(diff) == 1)
        {
            if (diff > 0)
                relation = JigBoundaryCollider.BoundarySide.right;
            else
                relation = JigBoundaryCollider.BoundarySide.left;
        }
        else if (Mathf.Abs(diff) == columns)
        {
            if (diff > 0)
                relation = JigBoundaryCollider.BoundarySide.top;
            else
                relation = JigBoundaryCollider.BoundarySide.bottom;
        }
        return relation;
    }
    private void OnPuzzleLoadingPercent(float percent)
    {
        percentLoaded = percent;
    }
    private void OnPuzzleLoadCompleted()
    {
        pieces = new JigPieceBehaviour[generatedPieces.Length];
        for (int i = 0; i < generatedPieces.Length; i++)
        {
            var pieceObject = generatedPieces[i];
            //piece.AddComponent<GrabbableTransform>();
            var pieceBehaviour = pieceObject.AddComponent<JigPieceBehaviour>();
            pieceBehaviour.onAttachAttempt += OnAttachAttempt;
            pieces[i] = pieceBehaviour;

            int columns = puzzlePieceCount.x;
            int rows = puzzlePieceCount.y;
            float pieceWidth = puzzleSize.x / columns;
            float pieceDepth = puzzleSize.y;
            float pieceHeight = puzzleSize.z / rows;
            float boundaryWidth = pieceWidth * pieceBoundaryPercent.x;
            float boundaryHeight = pieceHeight * pieceBoundaryPercent.y;

            GameObject currentBoundaryObject;
            JigBoundaryCollider currentBoundaryScript;
            currentBoundaryObject = new GameObject();
            currentBoundaryObject.name = "BottomBoundary";
            currentBoundaryObject.transform.SetParent(pieceObject.transform, false);
            currentBoundaryScript = currentBoundaryObject.AddComponent<JigBoundaryCollider>();
            currentBoundaryScript.jigPiece = pieceBehaviour;
            currentBoundaryScript.boundarySide = JigBoundaryCollider.BoundarySide.bottom;
            currentBoundaryScript.boxCenter = new Vector3(0, 0, -pieceHeight / 2);
            currentBoundaryScript.boxSize = new Vector3(boundaryWidth, pieceDepth, boundaryHeight);

            currentBoundaryObject = new GameObject();
            currentBoundaryObject.name = "RightBoundary";
            currentBoundaryObject.transform.SetParent(pieceObject.transform, false);
            currentBoundaryScript = currentBoundaryObject.AddComponent<JigBoundaryCollider>();
            currentBoundaryScript.jigPiece = pieceBehaviour;
            currentBoundaryScript.boundarySide = JigBoundaryCollider.BoundarySide.right;
            currentBoundaryScript.boxCenter = new Vector3(pieceWidth / 2, 0, 0);
            currentBoundaryScript.boxSize = new Vector3(boundaryHeight, pieceDepth, boundaryWidth);

            currentBoundaryObject = new GameObject();
            currentBoundaryObject.name = "TopBoundary";
            currentBoundaryObject.transform.SetParent(pieceObject.transform, false);
            currentBoundaryScript = currentBoundaryObject.AddComponent<JigBoundaryCollider>();
            currentBoundaryScript.jigPiece = pieceBehaviour;
            currentBoundaryScript.boundarySide = JigBoundaryCollider.BoundarySide.top;
            currentBoundaryScript.boxCenter = new Vector3(0, 0, pieceHeight / 2);
            currentBoundaryScript.boxSize = new Vector3(boundaryWidth, pieceDepth, boundaryHeight);

            currentBoundaryObject = new GameObject();
            currentBoundaryObject.name = "LeftBoundary";
            currentBoundaryObject.transform.SetParent(pieceObject.transform, false);
            currentBoundaryScript = currentBoundaryObject.AddComponent<JigBoundaryCollider>();
            currentBoundaryScript.jigPiece = pieceBehaviour;
            currentBoundaryScript.boundarySide = JigBoundaryCollider.BoundarySide.left;
            currentBoundaryScript.boxCenter = new Vector3(-pieceWidth / 2, 0, 0);
            currentBoundaryScript.boxSize = new Vector3(boundaryHeight, pieceDepth, boundaryWidth);

            var pieceCluster = CreateNewCluster();
            var randX = (Random.value * 2 - 1) * boardOuterBorder.x;
            var randY = (Random.value * 2 - 1) * boardOuterBorder.y;
            bool xInside = randX > -boardInnerBorder.x && randX < boardInnerBorder.x;
            bool yInside = randY > -boardInnerBorder.y && randY < boardInnerBorder.y;
            if (xInside || yInside)
            {
                var rightSideDiff = boardInnerBorder.x - randX;
                var leftSideDiff = -boardInnerBorder.x - randX;
                var topSideDiff = boardInnerBorder.y - randY;
                var bottomSideDiff = -boardInnerBorder.y - randY;

                bool rightLessLeft = Mathf.Abs(rightSideDiff) < Mathf.Abs(leftSideDiff);
                bool rightLessTop = Mathf.Abs(rightSideDiff) < Mathf.Abs(topSideDiff);
                bool rightLessBottom = Mathf.Abs(rightSideDiff) < Mathf.Abs(bottomSideDiff);
                bool leftLessTop = Mathf.Abs(leftSideDiff) < Mathf.Abs(topSideDiff);
                bool leftLessBottom = Mathf.Abs(leftSideDiff) < Mathf.Abs(bottomSideDiff);
                bool topLessBottom = Mathf.Abs(topSideDiff) < Mathf.Abs(bottomSideDiff);
                if ((xInside && rightLessLeft) && (!yInside || (rightLessTop && rightLessBottom)))
                    randX += rightSideDiff + pieceWidth / 2;
                else if (xInside && (!yInside || (leftLessTop && leftLessBottom)))
                    randX += leftSideDiff - pieceWidth / 2;
                else if (yInside && topLessBottom)
                    randY += topSideDiff + pieceHeight / 2;
                else if (yInside)
                    randY += bottomSideDiff - pieceHeight / 2;
            }
            pieceCluster.position = new Vector3(randX, 0, randY);
            pieceObject.transform.position = pieceCluster.position;
            AddPieceToCluster(pieceCluster, i);
        }
            
        isLoading = false;
        isLoaded = true;
        OnPuzzleLoaded?.Invoke();
    }
}
