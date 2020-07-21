using UnityEngine;
using UnityHelpers;

public class JigsawGame : MonoBehaviour
{
    public Vector3 puzzleSize = new Vector3(1, 0.1f, 1);
    public Vector2Int puzzlePieceCount = new Vector2Int(5, 5);
    public int seed = 1337;
    public float percentLoaded;

    [Space(10)]
    public Material puzzleFaceMat;
    public Material puzzleSideMat;
    public Material puzzleBackMat;

    [Space(10)]
    public UnityEngine.Events.UnityEvent OnPuzzleLoaded;

    private Transform jigsawParent;
    private GameObject[] pieces;

    void Awake()
    {
        jigsawParent = new GameObject().transform;
        jigsawParent.name = "JigsawParent";
    }
    void Start()
    {
        LoadJigsawPuzzle();
    }

    public void SetPuzzleVisibility(bool onOff)
    {
        jigsawParent.gameObject.SetActive(onOff);
    }
    public void LoadJigsawPuzzle()
    {
        pieces = new GameObject[25];
        StartCoroutine(JigsawPuzzle.Generate(puzzlePieceCount.x, puzzlePieceCount.y, puzzleSize.x, puzzleSize.z, puzzleSize.y, 5, seed, jigsawParent, pieces, puzzleFaceMat, puzzleSideMat, puzzleBackMat, true, OnPuzzleLoadingPercent, OnPuzzleLoadCompleted));
    }

    private void OnPuzzleLoadingPercent(float percent)
    {
        percentLoaded = percent;
    }
    private void OnPuzzleLoadCompleted()
    {
        foreach (var piece in pieces)
            piece.AddComponent<GrabbableTransform>();
            
        OnPuzzleLoaded?.Invoke();
    }
}
